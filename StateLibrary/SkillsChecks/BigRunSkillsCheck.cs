using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if the ball carrier breaks into the secondary for a big run
    /// </summary>
    public class BigRunSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _ballCarrier;

        public BigRunSkillsCheck(ISeedableRandom rng, Player ballCarrier)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
        }

        public override void Execute(Game game)
        {
            // Base chance for a big run, increased by speed above threshold
            var speedBonus = (_ballCarrier.Speed - GameProbabilities.Rushing.BIG_RUN_SPEED_THRESHOLD)
                / GameProbabilities.Rushing.BIG_RUN_SPEED_DENOMINATOR;
            var bigRunProbability = GameProbabilities.Rushing.BIG_RUN_BASE_PROBABILITY + speedBonus;

            // Clamp to reasonable bounds
            bigRunProbability = Math.Max(
                GameProbabilities.Rushing.BIG_RUN_MIN_CLAMP,
                Math.Min(GameProbabilities.Rushing.BIG_RUN_MAX_CLAMP, bigRunProbability));

            Occurred = _rng.NextDouble() < bigRunProbability;
        }
    }
}
