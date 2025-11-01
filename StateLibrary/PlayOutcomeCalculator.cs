using DomainObjects;
using DomainObjects.Helpers;

namespace StateLibrary
{
    /// <summary>
    /// Calculates realistic play outcomes based on player skills, game situation, and randomness
    /// </summary>
    public class PlayOutcomeCalculator
    {
        private readonly ISeedableRandom _random;

        public PlayOutcomeCalculator(ISeedableRandom random)
        {
            _random = random;
        }

        /// <summary>
        /// Calculate yards gained on a running play
        /// </summary>
        public int CalculateRunYardage(Game game)
        {
            var play = game.CurrentPlay;

            // Find the ball carrier (RB or QB)
            var ballCarrier = play.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.RB)
                ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);

            if (ballCarrier == null)
                return 0;

            // Calculate offensive power (ball carrier + blockers)
            var offensivePower = CalculateOffensivePower(play.OffensePlayersOnField, ballCarrier);

            // Calculate defensive power
            var defensivePower = CalculateDefensivePower(play.DefensePlayersOnField);

            // Calculate base yardage (with randomness)
            var skillDifferential = offensivePower - defensivePower;
            var baseYards = 3.0 + (skillDifferential / 20.0); // Average around 3-5 yards

            // Add randomness (-3 to +8 yard variance)
            var randomFactor = (_random.NextDouble() * 11) - 3;
            var totalYards = baseYards + randomFactor;

            // Occasionally get big runs (10% chance of breakaway if speed is high)
            if (_random.NextDouble() < 0.10 && ballCarrier.Speed > 75)
            {
                totalYards += _random.Next(10, 40); // Breakaway run
            }

            // Clamp to reasonable values (-5 to remaining distance to goal)
            var yardsToGoal = 100 - game.FieldPosition;
            return Math.Max(-5, Math.Min((int)Math.Round(totalYards), yardsToGoal));
        }

        /// <summary>
        /// Calculate yards gained on a passing play
        /// </summary>
        public int CalculatePassYardage(Game game, out bool isComplete)
        {
            var play = game.CurrentPlay;

            // Find QB and receivers
            var qb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
            var receivers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.WR ||
                p.Position == Positions.TE ||
                p.Position == Positions.RB).ToList();

            if (qb == null || !receivers.Any())
            {
                isComplete = false;
                return 0;
            }

            // Calculate completion probability based on QB passing, receiver catching, and defensive coverage
            var offensivePassPower = (qb.Passing * 2 + receivers.Average(r => r.Catching)) / 3.0;
            var defensivePassPower = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.CB || p.Position == Positions.S || p.Position == Positions.FS)
                .Average(p => p.Coverage);

            var completionChance = 0.60 + ((offensivePassPower - defensivePassPower) / 200.0); // Base 60% completion
            completionChance = Math.Max(0.30, Math.Min(0.85, completionChance)); // Clamp between 30% and 85%

            isComplete = _random.NextDouble() < completionChance;

            if (!isComplete)
            {
                return 0; // Incomplete pass
            }

            // Calculate yardage on completed pass
            var targetReceiver = receivers[_random.Next(receivers.Count)];
            var baseYards = 7.0 + ((qb.Passing + targetReceiver.Catching + targetReceiver.Speed) / 50.0);

            // Add randomness
            var randomFactor = (_random.NextDouble() * 14) - 2; // -2 to +12 variance
            var totalYards = baseYards + randomFactor;

            // Occasionally get long bombs (5% chance if receiver has high speed and catching)
            if (_random.NextDouble() < 0.05 && targetReceiver.Speed > 80 && targetReceiver.Catching > 75)
            {
                totalYards += _random.Next(20, 60); // Deep ball completion
            }

            // Clamp to reasonable values (0 to remaining distance to goal)
            var yardsToGoal = 100 - game.FieldPosition;
            return Math.Max(0, Math.Min((int)Math.Round(totalYards), yardsToGoal));
        }

        /// <summary>
        /// Calculate kickoff return yardage
        /// </summary>
        public int CalculateKickoffReturnYardage(Game game)
        {
            var play = game.CurrentPlay;
            var returner = play.OffensePlayersOnField.FirstOrDefault(p =>
                p.Position == Positions.WR || p.Position == Positions.RB);

            if (returner == null)
                return 20; // Touchback default

            // Base return around 20-25 yards
            var baseYards = 20.0 + (returner.Speed / 10.0);
            var randomFactor = (_random.NextDouble() * 12) - 6; // -6 to +6 variance
            var totalYards = baseYards + randomFactor;

            // Occasionally break a big return (3% chance)
            if (_random.NextDouble() < 0.03 && returner.Speed > 85)
            {
                totalYards += _random.Next(30, 80); // Big return or TD
            }

            // Kickoff returns start at own 0, can't go past 100
            return Math.Max(0, Math.Min((int)Math.Round(totalYards), 100));
        }

        /// <summary>
        /// Calculate punt distance
        /// </summary>
        public int CalculatePuntDistance(Game game)
        {
            var play = game.CurrentPlay;
            var punter = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.P);

            if (punter == null)
                return 35; // Default punt

            // Base punt around 40-45 yards based on kicking skill
            var baseDistance = 35.0 + (punter.Kicking / 5.0);
            var randomFactor = (_random.NextDouble() * 10) - 5; // -5 to +5 variance
            var totalDistance = baseDistance + randomFactor;

            return Math.Max(20, Math.Min((int)Math.Round(totalDistance), 65)); // Punts range 20-65 yards
        }

        /// <summary>
        /// Calculate punt return yardage
        /// </summary>
        public int CalculatePuntReturnYardage(Game game)
        {
            var play = game.CurrentPlay;
            var returner = play.DefensePlayersOnField.FirstOrDefault(p =>
                p.Position == Positions.WR || p.Position == Positions.RB || p.Position == Positions.CB);

            // 40% chance of fair catch
            if (_random.NextDouble() < 0.40)
                return 0;

            if (returner == null)
                return 5; // Default short return

            // Base return around 5-10 yards
            var baseYards = 5.0 + (returner.Speed / 15.0);
            var randomFactor = (_random.NextDouble() * 10) - 5; // -5 to +5 variance
            var totalYards = baseYards + randomFactor;

            // Occasionally break a big return (2% chance)
            if (_random.NextDouble() < 0.02 && returner.Speed > 85)
            {
                totalYards += _random.Next(30, 70); // Big return
            }

            return Math.Max(-5, Math.Min((int)Math.Round(totalYards), 50)); // Can lose yards, max 50 yard return
        }

        /// <summary>
        /// Determine if field goal is good
        /// </summary>
        public bool IsFieldGoalGood(Game game)
        {
            var play = game.CurrentPlay;
            var kicker = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.K);

            if (kicker == null)
                return false;

            // Calculate distance to goal (field position + 17 yards for end zone + snap)
            var distance = (100 - game.FieldPosition) + 17;

            // Base success probability diminishes with distance
            var baseChance = 0.95 - ((distance - 20) * 0.015); // 95% at 20 yards, drops 1.5% per yard

            // Adjust for kicker skill
            var skillAdjustment = (kicker.Kicking - 70) / 100.0; // +/- based on skill vs average
            var successChance = baseChance + skillAdjustment;

            // Clamp between 10% and 99%
            successChance = Math.Max(0.10, Math.Min(0.99, successChance));

            return _random.NextDouble() < successChance;
        }

        private double CalculateOffensivePower(List<Player> offensivePlayers, Player ballCarrier)
        {
            var blockers = offensivePlayers.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            var blockingPower = blockers.Any() ? blockers.Average(b => b.Blocking) : 50;
            var ballCarrierPower = (ballCarrier.Rushing * 2 + ballCarrier.Speed + ballCarrier.Agility) / 4.0;

            return (blockingPower + ballCarrierPower) / 2.0;
        }

        private double CalculateDefensivePower(List<Player> defensivePlayers)
        {
            var defenders = defensivePlayers.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            return defenders.Any() ? defenders.Average(d => (d.Tackling + d.Strength + d.Speed) / 3.0) : 50;
        }
    }
}
