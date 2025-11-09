using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

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

            var catchingFactor = _returner.Catching / 100.0;
            var baseMuffChance = 0.05 - (catchingFactor * 0.04); // 5% to 1%

            // Hang time pressure (longer hang time = defenders closer = more pressure)
            if (_hangTime > 4.5)
                baseMuffChance += 0.02;
            else if (_hangTime > 4.0)
                baseMuffChance += 0.01;

            Occurred = _rng.NextDouble() < baseMuffChance;
        }
    }
}
