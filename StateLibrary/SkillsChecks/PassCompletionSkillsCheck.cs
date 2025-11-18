using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a pass is completed based on QB passing, receiver catching, and coverage
    /// </summary>
    public class PassCompletionSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _qb;
        private Player _receiver;
        private bool _underPressure;

        public PassCompletionSkillsCheck(ISeedableRandom rng, Player qb, Player receiver, bool underPressure)
        {
            _rng = rng;
            _qb = qb;
            _receiver = receiver;
            _underPressure = underPressure;
        }

        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate passing effectiveness
            var passingPower = (_qb.Passing * 2 + _qb.Awareness) / 3.0;

            // Calculate receiving effectiveness
            var receivingPower = (_receiver.Catching + _receiver.Speed + _receiver.Agility) / 3.0;

            // Calculate coverage effectiveness
            var defenders = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS ||
                p.Position == Positions.LB).ToList();

            var coveragePower = defenders.Any()
                ? defenders.Average(d => (d.Coverage + d.Speed + d.Awareness) / 3.0)
                : 50;

            // Calculate offensive power (QB + receiver)
            var offensivePower = (passingPower + receivingPower) / 2.0;

            // Calculate completion probability (base rate adjusted by skills)
            var skillDifferential = offensivePower - coveragePower;
            var completionProbability = GameProbabilities.Passing.COMPLETION_BASE_PROBABILITY
                + (skillDifferential / GameProbabilities.Passing.COMPLETION_SKILL_DENOMINATOR);

            // Pressure reduces completion chance significantly
            if (_underPressure)
            {
                completionProbability -= GameProbabilities.Passing.COMPLETION_PRESSURE_PENALTY;
            }

            // Clamp to reasonable bounds
            completionProbability = Math.Max(
                GameProbabilities.Passing.COMPLETION_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.COMPLETION_MAX_CLAMP, completionProbability));

            Occurred = _rng.NextDouble() < completionProbability;
        }
    }
}
