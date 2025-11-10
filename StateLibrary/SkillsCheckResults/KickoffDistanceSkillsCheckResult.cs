using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates kickoff distance based on kicker skill
    /// </summary>
    public class KickoffDistanceSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _kicker;

        public KickoffDistanceSkillsCheckResult(ISeedableRandom rng, Player kicker)
        {
            _rng = rng;
            _kicker = kicker;
        }

        public override void Execute(Game game)
        {
            // Base kickoff distance: 50-70 yards for average kicker
            // Kicker skill range: 0-100 (typically 40-90 for specialists)

            var kickerSkill = _kicker.Kicking;

            // Base distance centered around 60 yards
            var baseDistance = 40.0 + (kickerSkill / 100.0) * 30.0;  // 40-70 yards range

            // Random variance ±10 yards
            var randomFactor = (_rng.NextDouble() * 20.0) - 10.0;

            var totalDistance = baseDistance + randomFactor;

            // Clamp to realistic range (30-80 yards)
            Value = Math.Max(30.0, Math.Min(80.0, totalDistance));
        }
    }
}
