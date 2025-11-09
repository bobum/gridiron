using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

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

            var baseDownedChance = 0.15; // 15% baseline

            // Field position factor (from punting team's perspective)
            if (_puntLandingSpot > 95) // Inside opponent's 5
                baseDownedChance += 0.40;
            else if (_puntLandingSpot > 90) // Inside opponent's 10
                baseDownedChance += 0.25;
            else if (_puntLandingSpot > 85) // Inside opponent's 15
                baseDownedChance += 0.15;

            // Hang time factor (better coverage)
            if (_hangTime > 4.5)
                baseDownedChance += 0.10;
            else if (_hangTime > 4.0)
                baseDownedChance += 0.05;

            Occurred = _rng.NextDouble() < baseDownedChance;
        }
    }
}
