using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
using StateLibrary.SkillsCheckResults;
using System;
using System.Linq;

namespace StateLibrary.Plays
{
    /// <summary>
    /// Executes field goal and extra point attempts with all scenarios:
    /// - Bad snaps
    /// - Blocked kicks
    /// - Make/miss based on distance and kicker skill
    /// - Blocked kick returns
    /// </summary>
    public sealed class FieldGoal : IGameAction
    {
        private readonly ISeedableRandom _rng;

        public FieldGoal(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game)
        {
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Find the kicker
            var kicker = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.K);
            if (kicker == null)
            {
                // Use punter as backup kicker
                kicker = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.P);
            }
            if (kicker == null)
            {
                // Last resort: use any player
                kicker = play.OffensePlayersOnField.FirstOrDefault();
            }

            play.Kicker = kicker;

            // Find the holder
            var holder = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.P)
                ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
            play.Holder = holder;

            // Calculate attempt distance (from line of scrimmage + 17 yards for end zone depth and snap distance)
            play.AttemptDistance = (100 - game.FieldPosition) + 17;

            // Check for bad snap
            var longSnapper = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.LS)
                ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.C);

            var badSnapCheck = new BadSnapOccurredSkillsCheck(_rng, longSnapper);
            badSnapCheck.Execute(game);

            if (badSnapCheck.Occurred)
            {
                ExecuteBadSnap(game, play, holder, longSnapper);
                return;
            }

            play.GoodSnap = true;

            // Check for blocked field goal
            // Get offensive line and defensive rushers for block calculation
            var offensiveLine = play.OffensePlayersOnField
                .Where(p => p.Position == Positions.T || p.Position == Positions.G ||
                            p.Position == Positions.C || p.Position == Positions.TE)
                .ToList();

            var defensiveRushers = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                            p.Position == Positions.LB || p.Position == Positions.OLB)
                .ToList();

            var blockCheck = new FieldGoalBlockOccurredSkillsCheck(
                _rng,
                kicker,
                play.AttemptDistance,
                offensiveLine,
                defensiveRushers,
                play.GoodSnap);
            blockCheck.Execute(game);

            if (blockCheck.Occurred)
            {
                ExecuteBlockedFieldGoal(game, play, kicker);
                return;
            }

            // Execute normal field goal attempt
            ExecuteNormalFieldGoal(game, play, kicker);
        }

        private void ExecuteBadSnap(Game game, FieldGoalPlay play, Player? holder, Player? longSnapper)
        {
            play.GoodSnap = false;
            play.IsGood = false;

            // Similar to punt bad snap - yards lost
            var baseLoss = -5.0 - (_rng.NextDouble() * 10.0); // -5 to -15 yards
            var randomFactor = (_rng.NextDouble() * 5.0) - 2.5; // ±2.5 yards
            var totalLoss = baseLoss + randomFactor;

            // Can't lose more than current field position (would be safety)
            var maxLoss = -1 * game.FieldPosition;
            totalLoss = Math.Max(maxLoss, totalLoss);

            play.YardsGained = (int)Math.Round(totalLoss);

            // Check if it's a safety
            if (play.YardsGained <= -1 * game.FieldPosition)
            {
                play.IsSafety = true;
                play.Result.LogInformation($"BAD SNAP! The ball rolls into the end zone for a SAFETY!");
            }
            else if (holder != null)
            {
                play.Result.LogInformation($"BAD SNAP! {holder.LastName} scrambles but loses {Math.Abs(play.YardsGained)} yards!");
            }
            else
            {
                play.Result.LogInformation($"BAD SNAP! Ball rolls for a loss of {Math.Abs(play.YardsGained)} yards!");
            }

            // Bad snap takes 4-7 seconds (chaos)
            play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
            play.PossessionChange = true; // Turnover on downs
        }

        private void ExecuteBlockedFieldGoal(Game game, FieldGoalPlay play, Player? kicker)
        {
            play.Blocked = true;
            play.IsGood = false;

            // Find the blocker (defensive line or linebacker)
            var blocker = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.LB || p.Position == Positions.OLB)
                .OrderByDescending(p => p.Speed + p.Strength)
                .FirstOrDefault();

            play.BlockedBy = blocker;

            if (blocker != null)
            {
                play.Result.LogInformation($"BLOCKED! {blocker.LastName} gets a hand on it!");
            }
            else
            {
                play.Result.LogInformation($"FIELD GOAL is BLOCKED!");
            }

            // Determine recovery (50/50 base chance)
            var defenseRecoveryChance = 0.5;
            var defenseRecovers = _rng.NextDouble() < defenseRecoveryChance;

            if (defenseRecovers)
            {
                // Defense recovers - can advance for TD ("scoop and score")
                var recoverer = play.DefensePlayersOnField
                    .OrderByDescending(p => p.Speed + p.Awareness)
                    .FirstOrDefault();

                if (recoverer == null)
                {
                    recoverer = blocker ?? play.DefensePlayersOnField.FirstOrDefault();
                }

                play.RecoveredBy = recoverer;

                if (recoverer != null)
                {
                    // Calculate return yardage using skills check
                    var returnCheck = new BlockedFieldGoalReturnYardsSkillsCheckResult(_rng, recoverer);
                    returnCheck.Execute(game);

                    var returnYards = (int)returnCheck.Result;
                    play.RecoveryYards = returnYards;

                    // Calculate final field position (from kicking team's perspective)
                    var finalPosition = game.FieldPosition + returnYards;

                    // Check for touchdown
                    if (finalPosition >= 100)
                    {
                        play.IsTouchdown = true;
                        play.YardsGained = 100 - game.FieldPosition;
                        play.Result.LogInformation($"{recoverer.LastName} picks it up and takes it ALL THE WAY! TOUCHDOWN on the blocked field goal!");

                        // Create return segment for statistics
                        var segment = new ReturnSegment
                        {
                            BallCarrier = recoverer,
                            YardsGained = play.YardsGained,
                            EndedInFumble = false
                        };
                        play.BlockReturnSegments = new System.Collections.Generic.List<ReturnSegment> { segment };
                    }
                    // Check for safety (returned into kicking team's end zone)
                    else if (finalPosition <= 0)
                    {
                        play.IsSafety = true;
                        play.YardsGained = -1 * game.FieldPosition;
                        play.Result.LogInformation($"{recoverer.LastName} recovers in the end zone! SAFETY!");
                    }
                    else
                    {
                        play.YardsGained = returnYards;
                        play.Result.LogInformation($"{recoverer.LastName} recovers and returns it {returnYards} yards!");

                        // Create return segment for statistics
                        var segment = new ReturnSegment
                        {
                            BallCarrier = recoverer,
                            YardsGained = returnYards,
                            EndedInFumble = false
                        };
                        play.BlockReturnSegments = new System.Collections.Generic.List<ReturnSegment> { segment };
                    }
                }

                play.PossessionChange = true;
            }
            else
            {
                // Offense recovers (usually negative yards)
                var recoverer = play.OffensePlayersOnField
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault();

                if (recoverer == null && kicker != null)
                {
                    recoverer = kicker;
                }

                play.RecoveredBy = recoverer;

                // Offense recovery: -5 to -15 yards typical
                var recoveryYards = -5 - (int)(_rng.NextDouble() * 10);
                recoveryYards = Math.Max(-1 * game.FieldPosition, recoveryYards); // Can't go past own goal

                // Check for safety (recovered in own end zone)
                if (game.FieldPosition + recoveryYards <= 0)
                {
                    play.IsSafety = true;
                    play.YardsGained = -1 * game.FieldPosition;
                    if (recoverer != null)
                    {
                        play.Result.LogInformation($"{recoverer.LastName} falls on it in the end zone! SAFETY!");
                    }
                    else
                    {
                        play.Result.LogInformation($"Ball recovered in the end zone! SAFETY!");
                    }
                }
                else
                {
                    play.RecoveryYards = recoveryYards;
                    play.YardsGained = recoveryYards;
                    if (recoverer != null)
                    {
                        play.Result.LogInformation($"{recoverer.LastName} falls on it for the offense, loss of {Math.Abs(recoveryYards)} yards.");
                    }
                    else
                    {
                        play.Result.LogInformation($"Offense recovers the blocked kick, loss of {Math.Abs(recoveryYards)} yards.");
                    }
                }

                play.PossessionChange = true; // Turnover on downs (4th down FG attempt)
            }

            // Blocked kicks take 3-6 seconds
            play.ElapsedTime += 3.0 + (_rng.NextDouble() * 3.0);
        }

        private void ExecuteNormalFieldGoal(Game game, FieldGoalPlay play, Player? kicker)
        {
            if (kicker == null)
            {
                play.IsGood = false;
                play.Result.LogInformation($"No kicker available! Attempt fails!");
                play.ElapsedTime += 2.0;
                play.PossessionChange = true;
                return;
            }

            // Check if kick is good
            var makeCheck = new FieldGoalMakeOccurredSkillsCheck(_rng, kicker, play.AttemptDistance);
            makeCheck.Execute(game);

            play.IsGood = makeCheck.Occurred;

            if (play.IsGood)
            {
                if (play.IsExtraPoint)
                {
                    play.Result.LogInformation($"{kicker.LastName} kicks the extra point... it's GOOD!");
                }
                else
                {
                    play.Result.LogInformation($"{kicker.LastName} attempts a {play.AttemptDistance}-yard field goal... it's GOOD!");
                }
            }
            else
            {
                // Determine miss direction
                var missType = _rng.NextDouble();
                string missDirection;

                if (missType < 0.4)
                {
                    missDirection = "WIDE RIGHT";
                }
                else if (missType < 0.8)
                {
                    missDirection = "WIDE LEFT";
                }
                else
                {
                    missDirection = "SHORT";
                }

                if (play.IsExtraPoint)
                {
                    play.Result.LogInformation($"{kicker.LastName} extra point attempt is NO GOOD! {missDirection}!");
                }
                else
                {
                    play.Result.LogInformation($"{kicker.LastName} {play.AttemptDistance}-yard attempt is NO GOOD! {missDirection}!");
                }
            }

            // Field goal/PAT takes 2-3 seconds
            play.ElapsedTime += 2.0 + (_rng.NextDouble() * 1.0);

            // Possession changes regardless of make/miss (scoring or turnover on downs)
            play.PossessionChange = true;
        }
    }
}
