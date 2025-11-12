using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    /// <summary>
    /// Processes kickoff results: field position, scoring, possession changes
    /// </summary>
    public class KickoffResult : IGameAction
    {
        public void Execute(Game game)
        {
            var play = (KickoffPlay)game.CurrentPlay;

            // Set start field position (kickoffs start at 35-yard line for kicking team)
            play.StartFieldPosition = 35;

            // Handle safety (returner tackled in own end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Award 2 points to kicking team
                game.AddSafety(play.Possession);

                play.PossessionChange = true;
                return; // Safety ends the play immediately
            }

            // Handle touchback
            if (play.Touchback)
            {
                // Receiving team gets ball at their 25-yard line
                game.FieldPosition = 25;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.Result.LogInformation($"Touchback. Receiving team starts at the 25-yard line.");
                return;
            }

            // Handle out of bounds
            if (play.OutOfBounds)
            {
                // Receiving team gets ball at 40-yard line (penalty)
                game.FieldPosition = 40;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.Result.LogInformation($"Kickoff out of bounds. Ball placed at the 40-yard line.");
                return;
            }

            // Handle onside kick
            if (play.OnsideKick)
            {
                if (play.OnsideRecovered)
                {
                    // Kicking team recovered - they keep possession!
                    play.PossessionChange = false;
                    game.FieldPosition = play.EndFieldPosition;
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                    play.Result.LogInformation($"Onside kick RECOVERED by kicking team! Ball at the {game.FieldPosition}-yard line.");
                }
                else
                {
                    // Receiving team recovered
                    play.PossessionChange = true;
                    game.FieldPosition = play.EndFieldPosition;
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                    play.Result.LogInformation($"Receiving team recovers at the {game.FieldPosition}-yard line.");
                }
                return;
            }

            // Handle kickoff return touchdown
            if (play.IsTouchdown)
            {
                // Receiving team scores
                var receivingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(receivingTeam);

                game.FieldPosition = 100;
                play.PossessionChange = true; // Will kick off again after TD

                play.Result.LogInformation($"Kickoff return TOUCHDOWN!");
                return;
            }

            // Normal kickoff return - set field position
            game.FieldPosition = play.EndFieldPosition;
            play.PossessionChange = true;
            game.CurrentDown = Downs.First;
            game.YardsToGo = 10;

            play.Result.LogInformation($"Kickoff return. Ball at the {game.FieldPosition}-yard line. 1st and 10.");
        }
    }
}
