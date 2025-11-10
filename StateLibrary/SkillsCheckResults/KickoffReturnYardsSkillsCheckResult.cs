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

            // Random factor: ±15 yards (big plays or tackles at spot)
            var randomFactor = (_rng.NextDouble() * 30.0) - 15.0;

            var totalReturn = baseReturn + randomFactor;

            // Clamp to realistic range (-5 to 50 yards)
            // Negative returns represent tackles behind catch point
            Value = Math.Max(-5.0, Math.Min(50.0, totalReturn));
        }
    }
}
