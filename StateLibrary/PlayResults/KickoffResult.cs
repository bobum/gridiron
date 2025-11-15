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

            // Handle fair catch
            if (play.FairCatch)
            {
                // Ball is dead at spot of fair catch
                game.FieldPosition = play.EndFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                return; // Fair catch ends the play
            }

            // Handle muffed catch
            if (play.MuffedCatch)
            {
                // Ball is set at recovery spot, possession as determined by recovery
                game.FieldPosition = play.EndFieldPosition;
                play.PossessionChange = play.PossessionChange; // Already set in Kickoff.cs
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                return; // Muff ends the play
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
                // Check if this is a defensive TD from fumble recovery
                if (play.Fumbles.Count > 0 && play.PossessionChange)
                {
                    // Kicking team recovered fumble and scored
                    var kickingTeam = play.Possession;
                    game.AddTouchdown(kickingTeam);
                    play.Result.LogInformation($"FUMBLE RECOVERY TOUCHDOWN by the kicking team!");
                }
                else
                {
                    // Normal return TD - receiving team scores
                    var receivingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                    game.AddTouchdown(receivingTeam);
                    play.Result.LogInformation($"Kickoff return TOUCHDOWN!");
                }

                game.FieldPosition = 100;
                play.PossessionChange = true; // Will kick off again after TD
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
