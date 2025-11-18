using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.Services;
using System.Linq;

namespace StateLibrary.PlayResults
{
    /// <summary>
    /// Handles field goal and extra point result processing,
    /// including scoring and possession changes
    /// </summary>
    public class FieldGoalResult : IGameAction
    {
        public void Execute(Game game)
        {
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.StartFieldPosition = game.FieldPosition;

            // Handle safety (bad snap into end zone or tackled in end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Determine who gets the 2 points based on who recovered the ball
                Possession scoringTeam;

                if (play.Blocked && play.RecoveredBy != null)
                {
                    // For blocked kicks, check who actually recovered the ball
                    var recoveredByDefense = play.DefensePlayersOnField.Contains(play.RecoveredBy);

                    if (recoveredByDefense)
                    {
                        // Defense recovered and ran backwards into kicking team's end zone → offense scores
                        scoringTeam = play.Possession;
                    }
                    else
                    {
                        // Offense recovered in their own end zone → defense scores
                        scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                    }
                }
                else
                {
                    // Not blocked (bad snap or other scenario): offense tackled in own end zone → defense scores
                    scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                }

                game.AddSafety(scoringTeam);

                play.PossessionChange = true;
                return; // Safety ends the play immediately
            }

            // Handle blocked field goal
            if (play.Blocked)
            {
                HandleBlockedFieldGoalRecovery(game, play);
                return;
            }

            // Handle successful field goal or extra point
            if (play.IsGood)
            {
                if (play.IsExtraPoint)
                {
                    game.AddExtraPoint(play.Possession);
                }
                else
                {
                    game.AddFieldGoal(play.Possession);
                }

                // After scoring, possession changes to other team (kickoff)
                play.EndFieldPosition = game.FieldPosition; // Same field position
                play.PossessionChange = true;
            }
            else
            {
                // Missed field goal - possession changes, no score
                // Ball goes to other team at spot of kick (7 yards behind LOS, or 20 yard line if in red zone)
                // The kick is taken from 7 yards behind the line of scrimmage
                var kickSpot = Math.Max(0, game.FieldPosition - 7);

                // If kick was from inside opponent's 20 (red zone), defense gets it at their own 20
                // Otherwise, defense gets it at the spot of kick
                var defensiveFieldPosition = 100 - kickSpot;

                if (defensiveFieldPosition < 20)
                {
                    // Kicked from inside opponent's 20 (red zone) - defense gets it at their own 20
                    play.EndFieldPosition = 80;
                    game.FieldPosition = 80;
                }
                else
                {
                    // Defense gets it at spot of kick (7 yards behind LOS)
                    play.EndFieldPosition = kickSpot;
                    game.FieldPosition = kickSpot;
                }

                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }

            // Handle penalties if any occurred
            HandlePenalties(game, play);

            // Log result summary
            LogFieldGoalSummary(play);
        }

        private void HandlePenalties(Game game, FieldGoalPlay play)
        {
            // Check if there are any penalties on the play
            var hasAcceptedPenalties = play.Penalties != null && play.Penalties.Any();

            if (!hasAcceptedPenalties)
            {
                return; // No penalties, nothing to do
            }

            // Apply smart acceptance/decline logic to penalties
            var penaltyEnforcement = new PenaltyEnforcement(play.Result);
            ApplyPenaltyAcceptanceLogic(game, play, penaltyEnforcement);

            // Recheck after acceptance logic
            hasAcceptedPenalties = play.Penalties.Any(p => p.Accepted);

            if (!hasAcceptedPenalties)
            {
                return; // All penalties were declined
            }

            // Enforce penalties and get the result
            var enforcementResult = penaltyEnforcement.EnforcePenalties(game, play, play.YardsGained);

            // Special handling for PAT/2-point conversion attempts
            if (play.IsExtraPoint || play.IsTwoPointConversion)
            {
                HandlePATOrTwoPointPenalties(game, play, enforcementResult);
                return;
            }

            // Regular field goal penalty handling
            var finalFieldPosition = game.FieldPosition + enforcementResult.NetYards - play.YardsGained;

            // Bounds check
            if (finalFieldPosition >= 100)
            {
                // Penalty pushed team into end zone for TD (rare on FG but possible)
                play.IsTouchdown = true;
                game.FieldPosition = 100;
                var scoringTeam = play.Possession;
                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true;
                play.Result.LogInformation($"Penalty enforcement results in TOUCHDOWN!");
                return;
            }
            else if (finalFieldPosition <= 0)
            {
                // Penalty pushed team into own end zone for safety
                play.IsSafety = true;
                game.FieldPosition = 0;
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddSafety(scoringTeam);
                play.PossessionChange = true;
                play.Result.LogInformation($"Penalty enforcement results in SAFETY!");
                return;
            }

            game.FieldPosition = finalFieldPosition;

            // Apply down and distance from penalty enforcement
            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - rekick
                game.CurrentDown = Downs.Fourth; // FG attempts are always 4th down
                play.Result.LogInformation($"Offsetting penalties. Rekick from the {game.FieldPosition}.");
            }
            else if (enforcementResult.AutomaticFirstDown)
            {
                // Automatic first down from penalty (e.g., defensive penalty on FG attempt)
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = false; // Kicking team retains possession
                play.Result.LogInformation($"Automatic first down from penalty. Ball at the {game.FieldPosition} yard line.");
            }
            else
            {
                // Update down and distance from enforcement result
                game.CurrentDown = enforcementResult.NewDown;
                game.YardsToGo = enforcementResult.NewYardsToGo;
                play.Result.LogInformation($"After penalty: {FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FieldPosition}.");
            }
        }

        /// <summary>
        /// Special handling for penalties during PAT or 2-point conversion attempts.
        /// These follow different rules than regular plays.
        /// </summary>
        private void HandlePATOrTwoPointPenalties(Game game, FieldGoalPlay play, PenaltyEnforcementResult enforcementResult)
        {
            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - replay the attempt
                play.Result.LogInformation($"Offsetting penalties. Replaying the {(play.IsExtraPoint ? "extra point" : "2-point conversion")} attempt.");
                return;
            }

            // Update field position based on penalty
            var finalFieldPosition = game.FieldPosition + enforcementResult.NetYards;

            // Bounds check
            finalFieldPosition = Math.Max(1, Math.Min(99, finalFieldPosition));
            game.FieldPosition = finalFieldPosition;

            // PAT/2-point attempts can be replayed from new spot after penalty
            play.Result.LogInformation($"Penalty on {(play.IsExtraPoint ? "extra point" : "2-point conversion")} attempt. Ball moved to the {game.FieldPosition}. Replay the down.");
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// </summary>
        private void ApplyPenaltyAcceptanceLogic(Game game, FieldGoalPlay play, PenaltyEnforcement enforcement)
        {
            if (play.Penalties == null || !play.Penalties.Any())
                return;

            foreach (var penalty in play.Penalties)
            {
                // Determine which team committed the penalty
                var penalizedTeam = penalty.CalledOn;

                // Use smart acceptance logic
                penalty.Accepted = enforcement.ShouldAcceptPenalty(
                    game,
                    penalty,
                    penalizedTeam,
                    play.Possession,
                    play.YardsGained,
                    play.IsExtraPoint ? Downs.First : Downs.Fourth, // PAT is like first down, FG is 4th down
                    game.YardsToGo);
            }
        }

        private string FormatDown(Downs down)
        {
            return down switch
            {
                Downs.First => "1st",
                Downs.Second => "2nd",
                Downs.Third => "3rd",
                Downs.Fourth => "4th",
                _ => "?"
            };
        }

        private void HandleBlockedFieldGoalRecovery(Game game, FieldGoalPlay play)
        {
            if (play.IsTouchdown)
            {
                // Defensive touchdown on blocked FG return
                var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(defendingTeam);

                play.EndFieldPosition = 100;
                game.FieldPosition = 100;
                play.PossessionChange = true;
            }
            else if (play.IsSafety)
            {
                // Safety already handled in safety block above, but this is here for clarity
                // This shouldn't be reached since safety is handled first
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;
                play.PossessionChange = true;
            }
            else
            {
                // Normal recovery - set field position
                var newFieldPosition = Math.Max(0, Math.Min(100, game.FieldPosition + play.YardsGained));
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;

                // Check possession change (if defense recovered)
                var recoveredByDefense = play.RecoveredBy != null &&
                    play.DefensePlayersOnField.Contains(play.RecoveredBy);

                if (recoveredByDefense)
                {
                    // Defense recovered - they get possession
                    play.PossessionChange = true;
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                }
                else
                {
                    // Offense recovered - turnover on downs (FG attempts are on 4th down)
                    play.PossessionChange = true;
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                }
            }
        }

        private void LogFieldGoalSummary(FieldGoalPlay play)
        {
            if (play.Kicker != null)
            {
                if (play.Blocked)
                {
                    play.Result.LogInformation($"Blocked kick. No points scored.");
                }
                else if (play.IsGood)
                {
                    if (play.IsExtraPoint)
                    {
                        play.Result.LogInformation($"{play.Kicker.LastName}: Extra point GOOD.");
                    }
                    else
                    {
                        play.Result.LogInformation($"{play.Kicker.LastName}: {play.AttemptDistance}-yard field goal GOOD.");
                    }
                }
                else
                {
                    if (play.IsExtraPoint)
                    {
                        play.Result.LogInformation($"{play.Kicker.LastName}: Extra point MISSED.");
                    }
                    else
                    {
                        play.Result.LogInformation($"{play.Kicker.LastName}: {play.AttemptDistance}-yard field goal MISSED.");
                    }
                }
            }
        }
    }
}
