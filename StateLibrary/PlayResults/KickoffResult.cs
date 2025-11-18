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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

                // Handle penalties if any occurred
                HandlePenalties(game, play);
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

            // If the play already resulted in a TD or safety, and penalties are declined, don't modify
            if ((play.IsTouchdown || play.IsSafety) && play.Penalties.All(p => !p.Accepted))
            {
                return; // Scoring play stands, penalties declined
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

            // For kickoffs, penalty enforcement depends on which team committed the foul
            var kickingTeamPenalty = play.Penalties.Any(p => p.Accepted && p.CalledOn == play.Possession);
            var receivingTeamPenalty = play.Penalties.Any(p => p.Accepted && p.CalledOn != play.Possession);

            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - rekick from original spot (35)
                game.FieldPosition = 35;
                play.PossessionChange = false; // Kicking team rekicks
                play.Result.LogInformation($"Offsetting penalties. Rekick from the 35-yard line.");
            }
            else if (kickingTeamPenalty && !receivingTeamPenalty)
            {
                // Kicking team penalty - rekick from adjusted spot
                // Penalty against kicking team moves them BACK (worse field position for them)
                // e.g., offsides on kickoff: instead of kicking from 35, kick from 40
                // NetYards is negative for penalties against the team, so we negate it to get positive adjustment
                var kickoffSpot = 35 - enforcementResult.NetYards;
                kickoffSpot = Math.Max(20, Math.Min(50, kickoffSpot)); // Keep within reasonable bounds
                game.FieldPosition = kickoffSpot;
                play.PossessionChange = false; // Kicking team rekicks
                play.Result.LogInformation($"Penalty on kicking team. Rekick from the {game.FieldPosition}-yard line.");
            }
            else
            {
                // Receiving team penalty - enforce from the return spot
                var returnSpot = play.EndFieldPosition > 0 ? play.EndFieldPosition : game.FieldPosition;
                // NetYards is positive for penalties against opponent, but we want to subtract to move ball back
                var finalFieldPosition = returnSpot - enforcementResult.NetYards;

                // Bounds check
                if (finalFieldPosition >= 100)
                {
                    // Penalty pushed receiving team forward into end zone for TD
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
                    // Penalty pushed receiving team into own end zone for safety
                    play.IsSafety = true;
                    game.FieldPosition = 0;
                    game.AddSafety(play.Possession); // Kicking team gets safety
                    play.PossessionChange = true;
                    play.Result.LogInformation($"Penalty enforcement results in SAFETY!");
                    return;
                }

                game.FieldPosition = finalFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = true; // Normal change of possession after penalty enforcement
                play.Result.LogInformation($"Penalty on receiving team. Ball at the {game.FieldPosition} yard line. 1st and 10.");
            }
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// Skips penalties that already have an explicit Accepted value set.
        /// </summary>
        private void ApplyPenaltyAcceptanceLogic(Game game, KickoffPlay play, PenaltyEnforcement enforcement)
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
                    0, // Kickoffs don't have yards gained in the same way
                    Downs.First, // Kickoffs always result in first down
                    game.YardsToGo);
            }
        }
    }
}
