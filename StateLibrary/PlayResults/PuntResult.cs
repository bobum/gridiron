using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.Services;
using System;
using System.Linq;

namespace StateLibrary.PlayResults
{
    public class PuntResult : IGameAction
    {
        public void Execute(Game game)
        {
            var play = (PuntPlay)game.CurrentPlay;

            // Set start field position
            play.StartFieldPosition = game.FieldPosition;

            // Check for penalties first - they may negate or modify the result
            var hasPenalties = play.Penalties != null && play.Penalties.Any();
            if (hasPenalties)
            {
                // Handle penalties first to see if they affect the result
                HandlePenalties(game, play);

                // If penalties were accepted, they handle all game state, so we're done
                var hasAcceptedPenalties = play.Penalties.Any(p => p.Accepted);
                if (hasAcceptedPenalties)
                {
                    LogPuntSummary(play);
                    return;
                }
            }

            // Handle safety (bad snap into end zone or tackled in end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Determine who gets the 2 points
                // If NOT blocked: bad snap, offense tackled in own end zone → defense scores
                // If blocked and possession changed: defense recovered and ran backwards → offense scores
                // If blocked and possession didn't change: offense recovered in end zone → defense scores
                var scoringTeam = (play.Blocked && play.PossessionChange)
                    ? play.Possession  // Defense recovered and ran backwards, original team gets points
                    : (play.Possession == Possession.Home ? Possession.Away : Possession.Home); // Offense tackled in end zone, defense gets points

                game.AddSafety(scoringTeam);

                play.PossessionChange = true;
                return; // Safety ends the play immediately
            }

            // Handle different punt outcomes
            if (play.Blocked && play.RecoveredBy != null)
            {
                // Blocked punt was recovered - handle scoring and possession
                HandleBlockedPuntRecovery(game, play);
            }
            else if (play.MuffedCatch && play.RecoveredBy != null)
            {
                // Muffed catch was recovered
                HandleMuffedCatchRecovery(game, play);
            }
            else if (play.Touchback)
            {
                // Touchback - receiving team gets ball at their 20
                play.EndFieldPosition = 80; // From punting team's perspective (100 - 20)
                game.FieldPosition = 80;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else if (play.Downed || play.FairCatch)
            {
                // Punt downed or fair catch - possession changes at spot
                var newFieldPosition = game.FieldPosition + play.YardsGained;
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else if (play.ReturnSegments.Any())
            {
                // Punt return occurred
                HandlePuntReturn(game, play);
            }
            else
            {
                // Default case (shouldn't normally hit this)
                var newFieldPosition = game.FieldPosition + play.YardsGained;
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }

            // Log final punt summary
            LogPuntSummary(play);
        }

        private void HandlePenalties(Game game, PuntPlay play)
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

            // Update field position based on net yards (play result + penalties)
            var finalFieldPosition = game.FieldPosition + enforcementResult.NetYards - play.YardsGained;

            // Bounds check
            if (finalFieldPosition >= 100)
            {
                // Penalty pushed returner into end zone for TD
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true;
                return;
            }
            else if (finalFieldPosition <= 0)
            {
                // Penalty pushed team into own end zone for safety
                play.IsSafety = true;
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddSafety(scoringTeam);
                play.PossessionChange = true;
                return;
            }

            play.EndFieldPosition = finalFieldPosition;
            game.FieldPosition = finalFieldPosition;

            // Apply down and distance from penalty enforcement
            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - replay the down (rekick for punt)
                game.CurrentDown = Downs.Fourth; // Punts are always 4th down
                play.Result.LogInformation($"Offsetting penalties. Rekick from the {game.FieldPosition}.");
            }
            else if (enforcementResult.AutomaticFirstDown)
            {
                // Automatic first down from penalty (e.g., defensive penalty on punt)
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = false; // Punting team retains possession
                play.Result.LogInformation($"Automatic first down from penalty. Ball at the {game.FieldPosition} yard line.");
            }
            else
            {
                // Regular punt penalty - possession changes after enforcement
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = true; // Possession changes on punts (unless automatic first down for punting team)
                play.Result.LogInformation($"Penalty enforced. Ball at the {game.FieldPosition} yard line. 1st and 10.");
            }
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// </summary>
        private void ApplyPenaltyAcceptanceLogic(Game game, PuntPlay play, PenaltyEnforcement enforcement)
        {
            if (play.Penalties == null || !play.Penalties.Any())
                return;

            // Skip if already explicitly accepted or declined (e.g., in tests or scenarios)
            var hasExplicitAcceptance = play.Penalties.Any(p => p.Accepted);
            if (hasExplicitAcceptance)
            {
                // Don't override explicit acceptance decisions
                return;
            }

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
                    play.Down,
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

        private void HandleBlockedPuntRecovery(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check if recovery resulted in touchdown
            if (play.IsTouchdown && (newFieldPosition >= 100 || newFieldPosition <= 0))
            {
                // Set end position based on which end zone
                if (newFieldPosition >= 100)
                {
                    play.EndFieldPosition = 100;
                    game.FieldPosition = 100;
                }
                else // newFieldPosition <= 0 - ball in punting team's end zone
                {
                    play.EndFieldPosition = 0;
                    game.FieldPosition = 0;
                }

                // Determine who scored (defense if they recovered)
                var scoringTeam = play.PossessionChange
                    ? (play.Possession == Possession.Home ? Possession.Away : Possession.Home)
                    : play.Possession;

                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true; // TD always changes possession
            }
            else if (play.PossessionChange)
            {
                // Defense recovered but no TD
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else
            {
                // Offense recovered their own blocked punt
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                // Turnover on downs (4th down punt failed)
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
        }

        private void HandleMuffedCatchRecovery(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            if (play.PossessionChange)
            {
                // Receiving team recovered their own muff
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else
            {
                // Punting team recovered the muff - they keep possession!
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.Result.LogInformation($"Punting team retains possession after muffed catch recovery!");
            }
        }

        private void HandlePuntReturn(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check for touchdown on return
            if (play.IsTouchdown || newFieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                // Receiving team scored
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(scoringTeam);

                play.PossessionChange = true;
            }
            else
            {
                // Normal return, no TD
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
        }

        private void LogPuntSummary(PuntPlay play)
        {
            if (play.Punter != null)
            {
                var returner = play.InitialReturner;
                var totalReturn = play.TotalReturnYards;

                if (play.Blocked)
                {
                    play.Result.LogInformation($"Blocked punt. Net: {play.YardsGained} yards.");
                }
                else if (play.Touchback)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, touchback.");
                }
                else if (play.FairCatch)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, fair catch.");
                }
                else if (play.Downed)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, downed at {100 - play.DownedAtYardLine}.");
                }
                else if (returner != null)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, {totalReturn} yard return by {returner.LastName}. Net: {play.YardsGained} yards.");
                }
                else
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt. Net: {play.YardsGained} yards.");
                }
            }
        }
    }
}