using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

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

            var baseOutOfBoundsChance = GameProbabilities.Punts.PUNT_OOB_BASE;

            // Field position factor (directional punting near goal line)
            if (_puntLandingSpot > GameProbabilities.Punts.PUNT_OOB_INSIDE_10_THRESHOLD)
                baseOutOfBoundsChance += GameProbabilities.Punts.PUNT_OOB_INSIDE_10_BONUS;
            else if (_puntLandingSpot > GameProbabilities.Punts.PUNT_OOB_INSIDE_15_THRESHOLD)
                baseOutOfBoundsChance += GameProbabilities.Punts.PUNT_OOB_INSIDE_15_BONUS;

            Occurred = _rng.NextDouble() < baseOutOfBoundsChance;
        }
    }
}
