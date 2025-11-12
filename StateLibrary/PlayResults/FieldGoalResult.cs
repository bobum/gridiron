using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

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

            // Log result summary
            LogFieldGoalSummary(play);
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
