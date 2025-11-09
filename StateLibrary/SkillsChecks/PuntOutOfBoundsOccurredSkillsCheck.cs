using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Checks if punt goes out of bounds (no return possible).
    /// Punters may intentionally punt out of bounds in certain situations.
    /// </summary>
    public class PuntOutOfBoundsOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly int _puntLandingSpot;

        public PuntOutOfBoundsOccurredSkillsCheck(
            ISeedableRandom rng,
            int puntLandingSpot)
        {
            _rng = rng;
            _puntLandingSpot = puntLandingSpot;
        }

        public override void Execute(Game game)
        {
            // Punt out of bounds probability:
            // - Generally around 10-15%
            // - Slightly higher deep in opponent territory (directional punting)

            var baseOutOfBoundsChance = 0.12; // 12% baseline

            // Field position factor (directional punting near goal line)
            if (_puntLandingSpot > 90) // Inside opponent's 10
                baseOutOfBoundsChance += 0.08;
            else if (_puntLandingSpot > 85) // Inside opponent's 15
                baseOutOfBoundsChance += 0.05;

            Occurred = _rng.NextDouble() < baseOutOfBoundsChance;
        }
    }
}
