using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if an incomplete pass is intercepted by the defense
    /// Only called when a pass is incomplete
    /// </summary>
    public class InterceptionOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _qb;
        private Player _receiver;
        private bool _underPressure;

        public InterceptionOccurredSkillsCheck(ISeedableRandom rng, Player qb, Player receiver, bool underPressure)
        {
            _rng = rng;
            _qb = qb;
            _receiver = receiver;
            _underPressure = underPressure;
        }

        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate QB passing skill (lower is worse, increases INT chance)
            var qbPassing = _qb.Passing;
            var qbAwareness = _qb.Awareness;
            var qbSkill = (qbPassing * 2 + qbAwareness) / 3.0;

            // Calculate coverage effectiveness
            var defenders = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS ||
                p.Position == Positions.LB).ToList();

            var coverageSkill = defenders.Any()
                ? defenders.Average(d => (d.Coverage * 2 + d.Awareness + d.Agility) / 4.0)
                : 50;

            // Base interception probability: 3.5% of incomplete passes
            var interceptionProbability = 0.035;

            // Adjust based on skill differential
            var skillDiff = coverageSkill - qbSkill;
            interceptionProbability += skillDiff / 500.0; // +/-0.2% per 10 skill points difference

            // Pressure increases interception chance (bad throws)
            if (_underPressure)
            {
                interceptionProbability += 0.02; // +2% under pressure
            }

            // Clamp between 1% and 15%
            interceptionProbability = Math.Max(0.01, Math.Min(0.15, interceptionProbability));

            Occurred = _rng.NextDouble() < interceptionProbability;
        }
    }
}
