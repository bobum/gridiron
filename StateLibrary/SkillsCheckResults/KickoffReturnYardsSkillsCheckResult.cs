using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates kickoff return yardage based on returner skill
    /// </summary>
    public class KickoffReturnYardsSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;

        public KickoffReturnYardsSkillsCheckResult(ISeedableRandom rng, Player returner)
        {
            _rng = rng;
            _returner = returner;
        }

        public override void Execute(Game game)
        {
            // Average kickoff return: 20-25 yards in NFL
            // Returner skill factors: Speed and Agility

            var returnerSkill = (_returner.Speed + _returner.Agility) / 2.0;

            // Base return: 15-30 yards
            var baseReturn = 10.0 + (returnerSkill / 100.0) * 20.0;

            // Random factor: ±60 yards (allows for both tackles at spot and breakaway TDs)
            var randomFactor = (_rng.NextDouble() * 120.0) - 60.0;

            var totalReturn = baseReturn + randomFactor;

            // Clamp to realistic range (-5 to 85 yards)
            // Negative returns represent tackles behind catch point
            // Upper range allows for long return TDs
            Result = Math.Max(-5.0, Math.Min(85.0, totalReturn));
        }
    }
}
