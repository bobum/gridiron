using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
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

            // Calculate completion probability (60% base, adjusted by skills)
            var skillDifferential = offensivePower - coveragePower;
            var completionProbability = 0.60 + (skillDifferential / 250.0);

            // Pressure reduces completion chance significantly
            if (_underPressure)
            {
                completionProbability -= 0.20; // -20% when under pressure
            }

            // Clamp between 25% and 85%
            completionProbability = Math.Max(0.25, Math.Min(0.85, completionProbability));

            Occurred = _rng.NextDouble() < completionProbability;
        }
    }
}
