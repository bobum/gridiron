using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
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

            // Calculate break tackle probability (base rate for elite backs)
            var skillDifferential = ballCarrierPower - tacklerPower;
            var breakProbability = GameProbabilities.Rushing.TACKLE_BREAK_BASE_PROBABILITY
                + (skillDifferential / GameProbabilities.Rushing.TACKLE_BREAK_SKILL_DENOMINATOR);

            // Clamp to reasonable bounds
            breakProbability = Math.Max(
                GameProbabilities.Rushing.TACKLE_BREAK_MIN_CLAMP,
                Math.Min(GameProbabilities.Rushing.TACKLE_BREAK_MAX_CLAMP, breakProbability));

            Occurred = _rng.NextDouble() < breakProbability;
        }
    }
}
