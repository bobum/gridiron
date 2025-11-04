using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

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
            // Base 8% chance for a big run, increased by speed
            var speedBonus = (_ballCarrier.Speed - 70) / 500.0; // 0-6% bonus for speed above 70
            var bigRunProbability = 0.08 + speedBonus;

            // Clamp between 3% and 15%
            bigRunProbability = Math.Max(0.03, Math.Min(0.15, bigRunProbability));

            Occurred = _rng.NextDouble() < bigRunProbability;
        }
    }
}
