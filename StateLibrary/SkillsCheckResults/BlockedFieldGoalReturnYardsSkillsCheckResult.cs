using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates return yardage for blocked field goal recoveries
    /// </summary>
    public class BlockedFieldGoalReturnYardsSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;

        public BlockedFieldGoalReturnYardsSkillsCheckResult(ISeedableRandom rng, Player returner)
        {
            _rng = rng;
            _returner = returner;
        }

        public override void Execute(Game game)
        {
            // Average blocked FG return: 15-25 yards
            // Elite returns can go the distance (rare but possible)

            var returnerSkill = (_returner.Speed + _returner.Agility) / 2.0;

            // Base return: 10-25 yards based on returner skill
            var baseReturn = 5.0 + (returnerSkill / 100.0) * 20.0;

            // Random factor: ±50 yards
            // This allows for:
            // - Negative returns (tackled behind recovery spot)
            // - Big returns (40-60 yards)
            // - Full-field TD returns (rare but possible)
            var randomFactor = (_rng.NextDouble() * 100.0) - 50.0;

            var totalReturn = baseReturn + randomFactor;

            // Clamp to realistic range (-5 to 100 yards)
            // Negative = tackled behind recovery spot
            // 100 yards = full-field TD return (very rare)
            // Most returns will be in the 10-30 yard range
            Result = Math.Max(-5.0, Math.Min(100.0, totalReturn));
        }
    }
}
