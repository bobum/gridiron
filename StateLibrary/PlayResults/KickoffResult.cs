using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.Services;
using System.Linq;

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

            // Handle penalties if any occurred
            HandlePenalties(game, play);
        }

        private void HandlePenalties(Game game, KickoffPlay play)
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
            var enforcementResult = penaltyEnforcement.EnforcePenalties(game, play, 0);

            // Update field position based on penalty enforcement
            var finalFieldPosition = game.FieldPosition + enforcementResult.NetYards;

            // Bounds check
            if (finalFieldPosition >= 100)
            {
                // Penalty pushed returner into end zone for TD
                play.IsTouchdown = true;
                game.FieldPosition = 100;
                var receivingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(receivingTeam);
                play.PossessionChange = true;
                play.Result.LogInformation($"Penalty enforcement results in TOUCHDOWN!");
                return;
            }
            else if (finalFieldPosition <= 0)
            {
                // Penalty pushed team into own end zone for safety
                play.IsSafety = true;
                game.FieldPosition = 0;
                game.AddSafety(play.Possession);
                play.PossessionChange = true;
                play.Result.LogInformation($"Penalty enforcement results in SAFETY!");
                return;
            }

            game.FieldPosition = finalFieldPosition;

            // Apply down and distance from penalty enforcement
            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - rekick
                play.Result.LogInformation($"Offsetting penalties. Rekick from the {game.FieldPosition}.");
            }
            else
            {
                // Update down and distance
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.Result.LogInformation($"After penalty: Ball at the {game.FieldPosition} yard line. 1st and 10.");
            }
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// </summary>
        private void ApplyPenaltyAcceptanceLogic(Game game, KickoffPlay play, PenaltyEnforcement enforcement)
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
                    0, // Kickoffs don't have yards gained in the same way
                    Downs.First, // Kickoffs always result in first down
                    game.YardsToGo);
            }
        }
    }
}
