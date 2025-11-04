using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if the ball carrier breaks a tackle attempt
    /// </summary>
    public class TackleBreakSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _ballCarrier;

        public TackleBreakSkillsCheck(ISeedableRandom rng, Player ballCarrier)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
        }

        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate ball carrier's evasion power
            var ballCarrierPower = (_ballCarrier.Rushing + _ballCarrier.Strength + _ballCarrier.Agility) / 3.0;

            // Calculate tackler power (get primary tackler - closest defender)
            var tacklers = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.LB ||
                p.Position == Positions.DE ||
                p.Position == Positions.DT ||
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS).ToList();

            var tacklerPower = tacklers.Any()
                ? tacklers.Average(t => (t.Tackling + t.Strength + t.Speed) / 3.0)
                : 50;

            // Calculate break tackle probability (30% base for elite backs)
            var skillDifferential = ballCarrierPower - tacklerPower;
            var breakProbability = 0.25 + (skillDifferential / 250.0);

            // Clamp between 5% and 50%
            breakProbability = Math.Max(0.05, Math.Min(0.50, breakProbability));

            Occurred = _rng.NextDouble() < breakProbability;
        }
    }
}
