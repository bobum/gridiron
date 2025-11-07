using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if the offensive line successfully creates a running lane
    /// </summary>
    public class BlockingSuccessSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        public BlockingSuccessSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate offensive blocking power
            var blockers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            var offensiveBlockingPower = blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : 50;

            // Calculate defensive line power
            var defenders = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB).ToList();

            var defensivePower = defenders.Any()
                ? defenders.Average(d => (d.Tackling + d.Strength) / 2.0)
                : 50;

            // Calculate success probability (50% base, adjusted by skill differential)
            var skillDifferential = offensiveBlockingPower - defensivePower;
            var successProbability = 0.50 + (skillDifferential / 200.0);

            // Clamp between 20% and 80%
            successProbability = Math.Max(0.20, Math.Min(0.80, successProbability));

            Occurred = _rng.NextDouble() < successProbability;
        }
    }
}
