using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Checks if punt is downed by punting team before being returned.
    /// More likely when punt lands deep in opponent territory.
    /// </summary>
    public class PuntDownedOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly int _puntLandingSpot;
        private readonly double _hangTime;

        public PuntDownedOccurredSkillsCheck(
            ISeedableRandom rng,
            int puntLandingSpot,
            double hangTime)
        {
            _rng = rng;
            _puntLandingSpot = puntLandingSpot;
            _hangTime = hangTime;
        }

        public override void Execute(Game game)
        {
            // Punt more likely to be downed when:
            // - Landing deep in opponent territory (near goal line)
            // - Good hang time (coverage gets there fast)

            var baseDownedChance = GameProbabilities.Punts.PUNT_DOWNED_BASE;

            // Field position factor (from punting team's perspective)
            if (_puntLandingSpot > GameProbabilities.Punts.PUNT_DOWNED_INSIDE_5_THRESHOLD)
                baseDownedChance += GameProbabilities.Punts.PUNT_DOWNED_INSIDE_5_BONUS;
            else if (_puntLandingSpot > GameProbabilities.Punts.PUNT_DOWNED_INSIDE_10_THRESHOLD)
                baseDownedChance += GameProbabilities.Punts.PUNT_DOWNED_INSIDE_10_BONUS;
            else if (_puntLandingSpot > GameProbabilities.Punts.PUNT_DOWNED_INSIDE_15_THRESHOLD)
                baseDownedChance += GameProbabilities.Punts.PUNT_DOWNED_INSIDE_15_BONUS;

            // Hang time factor (better coverage)
            if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_THRESHOLD)
                baseDownedChance += GameProbabilities.Punts.PUNT_DOWNED_HIGH_HANG_BONUS;
            else if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_THRESHOLD)
                baseDownedChance += GameProbabilities.Punts.PUNT_DOWNED_MEDIUM_HANG_BONUS;

            Occurred = _rng.NextDouble() < baseDownedChance;
        }
    }
}
