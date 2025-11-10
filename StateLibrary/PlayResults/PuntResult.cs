using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
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

            // Handle safety (bad snap into end zone or tackled in end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Determine who gets the 2 points
                // If possession didn't change: defending team gets points (offense tackled in own end zone)
                // If possession changed: original team gets points (defense ran into their own end zone)
                var scoringTeam = play.PossessionChange
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