using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Linq;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates base yards gained on a run play based on player skills and matchups.
    /// Considers offensive power (ball carrier + blockers) vs defensive power,
    /// then adds randomness to create realistic variance in run outcomes.
    /// </summary>
    public class RunYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _ballCarrier;
        private readonly List<Player> _offensivePlayers;
        private readonly List<Player> _defensivePlayers;

        public RunYardsSkillsCheckResult(
            ISeedableRandom rng,
            Player ballCarrier,
            List<Player> offensivePlayers,
            List<Player> defensivePlayers)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
            _offensivePlayers = offensivePlayers;
            _defensivePlayers = defensivePlayers;
        }

        public override void Execute(Game game)
        {
            // Calculate offensive power (ball carrier + blockers)
            var offensivePower = CalculateOffensivePower();

            // Calculate defensive power
            var defensivePower = CalculateDefensivePower();

            // Calculate base yardage (with randomness)
            var skillDifferential = offensivePower - defensivePower;
            var baseYards = 3.0 + (skillDifferential / 20.0); // Average around 3-5 yards

            // Add randomness (-3 to +8 yard variance)
            var randomFactor = (_rng.NextDouble() * 11) - 3;
            var totalYards = baseYards + randomFactor;

            Result = (int)Math.Round(totalYards);
        }

        private double CalculateOffensivePower()
        {
            var blockers = _offensivePlayers.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            var blockingPower = blockers.Any() ? blockers.Average(b => b.Blocking) : 50;
            var ballCarrierPower = (_ballCarrier.Rushing * 2 + _ballCarrier.Speed + _ballCarrier.Agility) / 4.0;

            return (blockingPower + ballCarrierPower) / 2.0;
        }

        private double CalculateDefensivePower()
        {
            var defenders = _defensivePlayers.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            return defenders.Any() ? defenders.Average(d => (d.Tackling + d.Strength + d.Speed) / 3.0) : 50;
        }
    }
}
