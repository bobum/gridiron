using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
using System.Linq;

namespace StateLibrary.Plays
{
    //Run plays can be your typical, hand it off to the guy play
    //or a QB scramble
    //or a 2-pt conversion
    //or a kneel
    //a fake punt would be in the Punt class - those could be run or pass...
    //a muffed snap
    public sealed class Run : IGameAction
    {
        private ISeedableRandom _rng;
        private PlayOutcomeCalculator _calculator;

        public Run()
        {
            // Default constructor for backward compatibility
            _rng = new CryptoRandom();
            _calculator = new PlayOutcomeCalculator(_rng);
        }

        public Run(ISeedableRandom rng)
        {
            _rng = rng;
            _calculator = new PlayOutcomeCalculator(rng);
        }

        public void Execute(Game game)
        {
            var play = (RunPlay)game.CurrentPlay;

            // Determine the ball carrier (RB or QB for scramble)
            var ballCarrier = DetermineBallCarrier(play);

            if (ballCarrier == null)
            {
                play.Result.LogWarning("No ball carrier found for run play!");
                return;
            }

            // Determine run direction
            var direction = DetermineRunDirection();

            // Check if offensive line creates a good running lane
            var blockingCheck = new BlockingSuccessSkillsCheck(_rng);
            blockingCheck.Execute(game);

            var blockingSuccess = blockingCheck.Occurred;
            var blockingModifier = blockingSuccess ? 1.2 : 0.8; // +20% or -20% yards

            // Calculate base yardage using PlayOutcomeCalculator
            var baseYards = _calculator.CalculateRunYardage(game);
            var adjustedYards = (int)(baseYards * blockingModifier);

            // Check for tackle break (adds 3-8 yards)
            var tackleBreakCheck = new TackleBreakSkillsCheck(_rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            if (tackleBreakCheck.Occurred)
            {
                var tackleBreakYards = _rng.Next(3, 9);
                adjustedYards += tackleBreakYards;
                play.Result.LogInformation($"{ballCarrier.LastName} breaks a tackle! Keeps churning!");
            }

            // Check for big run breakaway
            var bigRunCheck = new BigRunSkillsCheck(_rng, ballCarrier);
            bigRunCheck.Execute(game);

            if (bigRunCheck.Occurred)
            {
                var breakawayYards = _rng.Next(15, 45);
                adjustedYards += breakawayYards;
                play.Result.LogInformation($"{ballCarrier.LastName} breaks into the open field! He's got room to run!");
            }

            // Ensure we don't exceed field boundaries
            var yardsToGoal = 100 - game.FieldPosition;
            var finalYards = Math.Max(-5, Math.Min(adjustedYards, yardsToGoal));

            // Create the run segment
            var segment = new RunSegment
            {
                BallCarrier = ballCarrier,
                YardsGained = finalYards,
                Direction = direction,
                EndedInFumble = false // Fumble check happens later in FumbleReturn state
            };

            play.RunSegments.Add(segment);
            play.YardsGained = finalYards;

            // Update elapsed time (run plays take 5-8 seconds)
            play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);

            // Log the play-by-play narrative
            LogRunPlayNarrative(play, ballCarrier, direction, blockingSuccess, finalYards, yardsToGoal);
        }

        private Player? DetermineBallCarrier(RunPlay play)
        {
            // Primary carrier is RB, but could be QB for scramble, or FB
            var rb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.RB);

            // 10% chance QB keeps it (scramble or option)
            if (_rng.NextDouble() < 0.10)
            {
                var qb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
                if (qb != null)
                    return qb;
            }

            // Default to RB
            return rb ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
        }

        private RunDirection DetermineRunDirection()
        {
            var directions = new[]
            {
                RunDirection.Left,
                RunDirection.Right,
                RunDirection.Middle,
                RunDirection.MiddleLeft,
                RunDirection.MiddleRight,
                RunDirection.UpTheMiddle,
                RunDirection.OffLeftTackle,
                RunDirection.OffRightTackle,
                RunDirection.Sweep
            };

            return directions[_rng.Next(directions.Length)];
        }

        private void LogRunPlayNarrative(RunPlay play, Player ballCarrier, RunDirection direction, bool blockingSuccess, int yards, int yardsToGoal)
        {
            var positionName = ballCarrier.Position == Positions.QB ? "quarterback" : "running back";
            var directionText = GetDirectionText(direction);

            if (blockingSuccess)
            {
                play.Result.LogInformation($"Great blocking up front! {ballCarrier.LastName} takes the handoff {directionText}");
            }
            else
            {
                play.Result.LogInformation($"Defenders penetrate the line! {ballCarrier.LastName} struggles {directionText}");
            }

            if (yards <= -2)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} is tackled in the backfield for a loss of {Math.Abs(yards)} yards!");
            }
            else if (yards <= 0)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} is stopped at the line of scrimmage!");
            }
            else if (yards <= 3)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} picks up {yards} yards before being brought down.");
            }
            else if (yards <= 8)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} finds a seam and gains {yards} yards!");
            }
            else if (yards <= 15)
            {
                play.Result.LogInformation($"Nice run by {ballCarrier.LastName}! Picks up {yards} yards!");
            }
            else if (yards < yardsToGoal)
            {
                play.Result.LogInformation($"BIG RUN! {ballCarrier.LastName} races for {yards} yards before being tackled!");
            }
            else
            {
                play.Result.LogInformation($"TOUCHDOWN!!! {ballCarrier.LastName} takes it {yards} yards to the house!");
            }
        }

        private string GetDirectionText(RunDirection direction)
        {
            return direction switch
            {
                RunDirection.Left => "to the left side",
                RunDirection.Right => "to the right side",
                RunDirection.Middle => "up the middle",
                RunDirection.MiddleLeft => "up the middle-left gap",
                RunDirection.MiddleRight => "up the middle-right gap",
                RunDirection.UpTheMiddle => "straight ahead",
                RunDirection.OffLeftTackle => "off left tackle",
                RunDirection.OffRightTackle => "off right tackle",
                RunDirection.Sweep => "on the sweep",
                _ => "forward"
            };
        }
    }
}
