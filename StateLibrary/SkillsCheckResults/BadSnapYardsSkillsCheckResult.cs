using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates yardage lost on a bad snap during a punt.
    /// Bad snaps can result in significant losses or even safeties.
    /// </summary>
    public class BadSnapYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly int _fieldPosition;

        public BadSnapYardsSkillsCheckResult(
            ISeedableRandom rng,
            int fieldPosition)
        {
            _rng = rng;
            _fieldPosition = fieldPosition;
        }

        public override void Execute(Game game)
        {
            // Bad snap scenarios:
            // - Punter recovers quickly: -2 to -8 yards
            // - Punter has to chase it down: -10 to -20 yards
            // - Ball rolls into end zone: potential safety

            var baseLoss = -5.0 - (_rng.NextDouble() * 15.0); // -5 to -20 yards

            // Add some randomness
            var randomFactor = (_rng.NextDouble() * 5.0) - 2.5; // ±2.5 yards
            var totalLoss = baseLoss + randomFactor;

            // Ensure we don't exceed field boundaries
            // Can't lose more yards than current field position (would be safety)
            var maxLoss = -1 * _fieldPosition;
            totalLoss = Math.Max(maxLoss, totalLoss);

            Result = (int)Math.Round(totalLoss);
        }
    }
}
