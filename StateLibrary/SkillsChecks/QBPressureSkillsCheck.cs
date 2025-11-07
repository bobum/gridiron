using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if the QB is under pressure (affects pass accuracy)
    /// </summary>
    public class QBPressureSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        public QBPressureSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate pass rush effectiveness (even if not sacked, QB can be pressured)
            var rushers = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            var passRushPower = rushers.Any()
                ? rushers.Average(r => (r.Speed + r.Strength) / 2.0)
                : 50;

            var blockers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T).ToList();

            var protectionPower = blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : 50;

            // Calculate pressure probability (30% base, adjusted by rush vs protection)
            var skillDifferential = passRushPower - protectionPower;
            var pressureProbability = 0.30 + (skillDifferential / 250.0);

            // Clamp between 10% and 60%
            pressureProbability = Math.Max(0.10, Math.Min(0.60, pressureProbability));

            Occurred = _rng.NextDouble() < pressureProbability;
        }
    }
}
