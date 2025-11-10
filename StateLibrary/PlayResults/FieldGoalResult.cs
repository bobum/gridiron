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

            // Handle safety (bad snap into end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Award 2 points to defending team
                var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddSafety(defendingTeam);

                play.PossessionChange = true;
                return; // Safety ends the play immediately
            }

            // Handle blocked kick scenarios
            if (play.Blocked)
            {
                HandleBlockedKick(game, play);
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
                // Ball goes to other team at spot of kick (or 20 yard line if beyond)
                var missedFGSpot = game.FieldPosition;

                // If kick was from beyond the 20 yard line, defense gets it at the spot
                // If kick was from inside the 20, defense gets it at the 20
                var defensiveFieldPosition = 100 - missedFGSpot;

                if (defensiveFieldPosition > 80)
                {
                    // Kicked from inside offensive 20 - defense gets it at their own 20
                    play.EndFieldPosition = 80;
                    game.FieldPosition = 80;
                }
                else
                {
                    // Defense gets it at spot of kick
                    play.EndFieldPosition = missedFGSpot;
                }

                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }

            // Log result summary
            LogFieldGoalSummary(play);
        }

        private void HandleBlockedKick(Game game, FieldGoalPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check if blocked kick was returned for touchdown
            if (play.IsTouchdown && (newFieldPosition <= 0 || newFieldPosition >= 100))
            {
                // Set end position based on which end zone
                if (newFieldPosition >= 100)
                {
                    play.EndFieldPosition = 100;
                    game.FieldPosition = 100;
                }
                else // newFieldPosition <= 0 - ball in kicking team's end zone
                {
                    play.EndFieldPosition = 0;
                    game.FieldPosition = 0;
                }

                // Defense scored
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true;
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
                // Offense recovered or ball is dead - turnover on downs
                play.EndFieldPosition = game.FieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
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
