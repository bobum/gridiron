using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
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
            var blockCheck = new FieldGoalBlockOccurredSkillsCheck(_rng);
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

            // Find the blocker
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

            // Determine if defense recovers and returns
            var defenseRecoveryChance = 0.4; // 40% chance defense recovers cleanly
            var defenseRecovers = _rng.NextDouble() < defenseRecoveryChance;

            if (defenseRecovers)
            {
                // Defense recovers and can return
                var returner = blocker ?? play.DefensePlayersOnField
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault();

                if (returner != null)
                {
                    // Calculate return yards
                    var baseReturn = -5.0 + (_rng.NextDouble() * 20.0); // -5 to +15 yards
                    var randomFactor = (_rng.NextDouble() * 10.0) - 5.0; // ±5 yards
                    var returnYards = baseReturn + randomFactor;

                    var finalPosition = game.FieldPosition + (int)returnYards;

                    // Check for touchdown
                    if (finalPosition <= 0)
                    {
                        play.IsTouchdown = true;
                        play.YardsGained = (int)returnYards;
                        play.Result.LogInformation($"{returner.LastName} returns the blocked kick for a TOUCHDOWN!");
                    }
                    else if (finalPosition >= 100)
                    {
                        play.IsTouchdown = true;
                        play.YardsGained = 100 - game.FieldPosition;
                        play.Result.LogInformation($"{returner.LastName} takes it to the house! TOUCHDOWN!");
                    }
                    else
                    {
                        play.YardsGained = Math.Min((int)returnYards, 100 - game.FieldPosition);
                        play.Result.LogInformation($"{returner.LastName} returns the blocked kick {play.YardsGained} yards!");
                    }
                }

                play.PossessionChange = true;
            }
            else
            {
                // Offense recovers or ball is dead
                play.YardsGained = 0;
                play.PossessionChange = true; // Turnover on downs
                play.Result.LogInformation($"Blocked kick falls incomplete.");
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
