using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Checks if returner signals and makes a fair catch.
    /// More likely with good hang time (better coverage) or deep in own territory.
    /// </summary>
    public class FairCatchOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly double _hangTime;
        private readonly int _returnSpot;

        public FairCatchOccurredSkillsCheck(
            ISeedableRandom rng,
            double hangTime,
            int returnSpot)
        {
            _rng = rng;
            _hangTime = hangTime;
            _returnSpot = returnSpot;
        }

        public override void Execute(Game game)
        {
            // Fair catch more likely with:
            // - Long hang time (good coverage breathing down neck)
            // - Deep in own territory (don't risk muff/loss)

            var baseFairCatchChance = 0.25; // 25% baseline

            // Hang time factor (longer hang time = more pressure)
            if (_hangTime > 4.5)
                baseFairCatchChance += 0.15;
            else if (_hangTime > 4.0)
                baseFairCatchChance += 0.10;

            // Field position factor (deep in own territory = more conservative)
            // returnSpot is from opponent's perspective (100 - their field position)
            var actualFieldPosition = 100 - _returnSpot;
            if (actualFieldPosition < 10) // Own 10 or closer
                baseFairCatchChance += 0.20;
            else if (actualFieldPosition < 20) // Own 20 or closer
                baseFairCatchChance += 0.10;

            Occurred = _rng.NextDouble() < baseFairCatchChance;
        }
    }
}
