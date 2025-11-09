using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Linq;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates yards gained on a punt return based on returner skills,
    /// hang time (coverage), and defensive matchup.
    /// </summary>
    public class PuntReturnYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;
        private readonly double _hangTime;
        private readonly List<Player> _coverage;

        public PuntReturnYardsSkillsCheckResult(
            ISeedableRandom rng,
            Player returner,
            double hangTime,
            List<Player> coverage)
        {
            _rng = rng;
            _returner = returner;
            _hangTime = hangTime;
            _coverage = coverage;
        }

        public override void Execute(Game game)
        {
            // Calculate returner's ability (speed + agility + catching)
            var returnerAbility = (_returner.Speed + _returner.Agility + _returner.Catching) / 3.0;

            // Calculate coverage quality (hang time affects coverage)
            // Longer hang time = better coverage
            var coverageQuality = CalculateCoverageQuality();

            // Base return: 0-15 yards typically
            var skillDifferential = (returnerAbility - coverageQuality) / 10.0;
            var baseReturn = 5.0 + skillDifferential;

            // Add randomness (-5 to +15 yard variance)
            var randomFactor = (_rng.NextDouble() * 20.0) - 5.0;
            var totalReturn = baseReturn + randomFactor;

            // Ensure minimum (can lose yards on return, but rarely)
            totalReturn = Math.Max(-3.0, totalReturn);

            Result = (int)Math.Round(totalReturn);
        }

        private double CalculateCoverageQuality()
        {
            // Better hang time = better coverage (more time to get downfield)
            var hangTimeFactor = Math.Min(_hangTime / 5.0, 1.0); // Cap at 5 seconds

            var coveragePlayers = _coverage.Where(p =>
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            var coverageSkill = coveragePlayers.Any()
                ? coveragePlayers.Average(p => (p.Speed + p.Tackling) / 2.0)
                : 50.0;

            // Hang time adds 0-20 points to coverage quality
            return coverageSkill + (hangTimeFactor * 20.0);
        }
    }
}
