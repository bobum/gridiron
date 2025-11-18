using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

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

            var baseFairCatchChance = GameProbabilities.Punts.PUNT_FAIR_CATCH_BASE;

            // Hang time factor (longer hang time = more pressure)
            if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_THRESHOLD)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_HIGH_HANG_BONUS;
            else if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_THRESHOLD)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_MEDIUM_HANG_BONUS;

            // Field position factor (deep in own territory = more conservative)
            var actualFieldPosition = 100 - _returnSpot;
            if (actualFieldPosition < 10)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_10_BONUS;
            else if (actualFieldPosition < 20)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_20_BONUS;

            Occurred = _rng.NextDouble() < baseFairCatchChance;
        }
    }
}
