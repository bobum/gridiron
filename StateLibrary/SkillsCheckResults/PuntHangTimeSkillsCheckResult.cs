using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates hang time (time ball is in air) based on punt distance.
    /// Longer punts generally have more hang time.
    /// Returns hang time as a double (in seconds).
    /// </summary>
    public class PuntHangTimeSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly int _puntDistance;

        public PuntHangTimeSkillsCheckResult(
            ISeedableRandom rng,
            int puntDistance)
        {
            _rng = rng;
            _puntDistance = puntDistance;
        }

        public override void Execute(Game game)
        {
            // Hang time formula: roughly 0.08-0.10 seconds per yard
            // 40-yard punt: ~3.2-4.0 seconds
            // 50-yard punt: ~4.0-5.0 seconds

            var baseHangTime = _puntDistance * 0.08;

            // Add randomness (±0.5 seconds)
            var randomFactor = (_rng.NextDouble() - 0.5);
            var totalHangTime = baseHangTime + randomFactor;

            // Ensure minimum hang time
            totalHangTime = Math.Max(2.0, totalHangTime);

            Result = Math.Round(totalHangTime, 1);
        }
    }
}
