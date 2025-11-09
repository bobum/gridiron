using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates the distance a punt travels based on punter's kicking skill
    /// and field position constraints.
    /// </summary>
    public class PuntDistanceSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _punter;
        private readonly int _fieldPosition;

        public PuntDistanceSkillsCheckResult(
            ISeedableRandom rng,
            Player punter,
            int fieldPosition)
        {
            _rng = rng;
            _punter = punter;
            _fieldPosition = fieldPosition;
        }

        public override void Execute(Game game)
        {
            // Base punt distance: 35-50 yards for average punter (kicking skill 50)
            // Better punters (70+ kicking): 45-60 yards
            // Weaker punters (30 kicking): 25-40 yards

            var skillFactor = _punter.Kicking / 100.0;
            var baseDistance = 30.0 + (skillFactor * 25.0); // 30-55 yard base

            // Add randomness (-10 to +15 yard variance for realistic variance)
            var randomFactor = (_rng.NextDouble() * 25.0) - 10.0;
            var totalDistance = baseDistance + randomFactor;

            // Ensure minimum punt distance (shanked punt: 10 yards)
            totalDistance = Math.Max(10.0, totalDistance);

            // Clamp to field boundaries
            // Can't punt beyond opponent's end zone (110 yards - field position gives max distance to back of end zone)
            var maxDistance = 110 - _fieldPosition;
            totalDistance = Math.Min(totalDistance, maxDistance);

            Result = (int)Math.Round(totalDistance);
        }
    }
}
