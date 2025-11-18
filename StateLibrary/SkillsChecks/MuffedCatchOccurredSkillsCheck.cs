using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Checks if returner muffs (drops) the punt.
    /// Muffed punts can be recovered by either team.
    /// </summary>
    public class MuffedCatchOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;
        private readonly double _hangTime;

        public MuffedCatchOccurredSkillsCheck(
            ISeedableRandom rng,
            Player returner,
            double hangTime)
        {
            _rng = rng;
            _returner = returner;
            _hangTime = hangTime;
        }

        public override void Execute(Game game)
        {
            // Muff probability based on:
            // - Returner's catching skill (lower = more muffs)
            // - Hang time (longer = more pressure)
            // - Random chance

            var catchingFactor = _returner.Catching / GameProbabilities.Punts.PUNT_MUFF_SKILL_DENOMINATOR;
            var baseMuffChance = GameProbabilities.Punts.PUNT_MUFF_BASE
                - (catchingFactor * GameProbabilities.Punts.PUNT_MUFF_SKILL_FACTOR);

            // Hang time pressure (longer hang time = defenders closer = more pressure)
            if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_THRESHOLD)
                baseMuffChance += GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_TIME_BONUS;
            else if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_THRESHOLD)
                baseMuffChance += GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_TIME_BONUS;

            Occurred = _rng.NextDouble() < baseMuffChance;
        }
    }
}
