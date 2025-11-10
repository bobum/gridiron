using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;
using System;
using System.Linq;

namespace StateLibrary.Plays
{
    /// <summary>
    /// Handles kickoff execution: normal kickoffs, touchbacks, onside kicks, and returns
    /// </summary>
    public sealed class Kickoff : IGameAction
    {
        private readonly ISeedableRandom _rng;

        public Kickoff(ISeedableRandom rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public void Execute(Game game)
        {
            var play = (KickoffPlay)game.CurrentPlay;

            // Find the kicker (should be from kicking team's special teams)
            var kicker = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.K)
                ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.P);

            if (kicker == null)
            {
                // No kicker available - shouldn't happen, but handle gracefully
                play.Result.LogInformation("No kicker available for kickoff!");
                play.KickDistance = 40; // Short kick
                play.Touchback = true;
                play.ElapsedTime += 3.0;
                return;
            }

            play.Kicker = kicker;

            // Check if this should be an onside kick (trailing late in game)
            // Simple heuristic for now
            if (ShouldAttemptOnsideKick(game))
            {
                play.OnsideKick = true;
                ExecuteOnsideKick(game, play, kicker);
                return;
            }

            // Execute normal kickoff
            ExecuteNormalKickoff(game, play, kicker);
        }

        private bool ShouldAttemptOnsideKick(Game game)
        {
            // Simple heuristic: Attempt onside kick if trailing by 7+ points in 4th quarter
            // In a real implementation, this would be more sophisticated
            var scoreDifferential = (game.CurrentPlay.Possession == Possession.Home)
                ? (game.HomeScore - game.AwayScore)
                : (game.AwayScore - game.HomeScore);

            // Very low probability for now - can be adjusted
            return scoreDifferential < -7 && _rng.NextDouble() < 0.05;
        }

        private void ExecuteOnsideKick(Game game, KickoffPlay play, Player kicker)
        {
            // Onside kicks travel 10-15 yards minimum
            play.KickDistance = 10 + (int)(_rng.NextDouble() * 5);

            play.Result.LogInformation($"{kicker.LastName} attempts an ONSIDE KICK!");

            // Recovery probability based on kicker skill and defense
            // Onside kicks have roughly 20-30% success rate in NFL
            var recoveryProb = 0.20 + (kicker.Kicking / 100.0) * 0.10;

            play.OnsideRecovered = _rng.NextDouble() < recoveryProb;

            if (play.OnsideRecovered)
            {
                // Kicking team recovered!
                play.OnsideRecoveredBy = (play.Possession == Possession.Home) ? game.HomeTeam : game.AwayTeam;
                play.PossessionChange = false; // Kicking team keeps possession!

                // Find who recovered
                var recoverer = play.OffensePlayersOnField
                    .OrderByDescending(p => p.Speed + p.Agility)
                    .FirstOrDefault();

                play.RecoveredBy = recoverer;

                // Ball spotted where recovered (10-15 yards downfield from kickoff spot)
                var kickoffSpot = 35; // Standard kickoff from 35-yard line
                play.EndFieldPosition = Math.Min(100, kickoffSpot + play.KickDistance);

                play.Result.LogInformation($"RECOVERED by {recoverer?.LastName ?? "kicking team"}! Kicking team retains possession!");
            }
            else
            {
                // Receiving team recovered
                play.PossessionChange = true;

                var recoverer = play.DefensePlayersOnField
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault();

                play.RecoveredBy = recoverer;

                // Ball spotted where recovered
                var kickoffSpot = 35;
                play.EndFieldPosition = Math.Min(100, kickoffSpot + play.KickDistance);

                play.Result.LogInformation($"{recoverer?.LastName ?? "Receiving team"} recovers the onside kick!");
            }

            // Onside kicks take 4-6 seconds
            play.ElapsedTime += 4.0 + (_rng.NextDouble() * 2.0);
        }

        private void ExecuteNormalKickoff(Game game, KickoffPlay play, Player kicker)
        {
            // Calculate kick distance based on kicker skill
            var kickDistanceCheck = new KickoffDistanceSkillsCheckResult(_rng, kicker);
            kickDistanceCheck.Execute(game);

            play.KickDistance = (int)kickDistanceCheck.Value;

            // Kickoffs are from the 35-yard line
            var kickoffSpot = 35;
            var landingSpot = kickoffSpot + play.KickDistance;

            // Check for out of bounds
            if (CheckOutOfBounds(landingSpot))
            {
                play.OutOfBounds = true;
                play.PossessionChange = true;
                // Penalty: Receiving team gets ball at 40-yard line
                play.EndFieldPosition = 40;
                play.Result.LogInformation($"{kicker.LastName} kicks it out of bounds! Ball placed at the 40-yard line.");
                play.ElapsedTime += 3.0;
                return;
            }

            // Check for touchback
            if (landingSpot >= 100)
            {
                play.Touchback = true;
                play.PossessionChange = true;
                play.EndFieldPosition = 25; // Touchback comes out to 25-yard line
                play.Result.LogInformation($"{kicker.LastName} kicks it deep! Touchback. Ball at the 25-yard line.");
                play.ElapsedTime += 3.0;
                return;
            }

            // Normal return
            ExecuteKickoffReturn(game, play, landingSpot);
        }

        private bool CheckOutOfBounds(int landingSpot)
        {
            // Kicks between 30-70 yards have small chance of going out of bounds
            if (landingSpot < 65 || landingSpot > 95)
            {
                return _rng.NextDouble() < 0.03; // 3% chance
            }

            return _rng.NextDouble() < 0.10; // 10% chance in the danger zone
        }

        private void ExecuteKickoffReturn(Game game, KickoffPlay play, int landingSpot)
        {
            play.PossessionChange = true;

            // Find the returner
            var returner = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.WR || p.Position == Positions.RB || p.Position == Positions.CB)
                .OrderByDescending(p => p.Speed + p.Agility)
                .FirstOrDefault()
                ?? play.DefensePlayersOnField.FirstOrDefault();

            if (returner == null)
            {
                // No returner - ball downed where it lands
                play.EndFieldPosition = 100 - landingSpot;
                play.Result.LogInformation($"Kickoff lands and is downed at the {100 - landingSpot}-yard line.");
                play.ElapsedTime += 5.0;
                return;
            }

            // Calculate return yardage
            var returnCheck = new KickoffReturnYardsSkillsCheckResult(_rng, returner);
            returnCheck.Execute(game);

            var returnYards = (int)returnCheck.Value;

            // Create return segment
            var segment = new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = returnYards,
                EndedInFumble = false
            };

            play.ReturnSegments.Add(segment);

            // Calculate final field position (from receiving team's perspective)
            var fieldPosition = 100 - landingSpot + returnYards;

            // Check for touchdown
            if (fieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                play.Result.LogInformation($"{returner.LastName} takes it back for a TOUCHDOWN! {landingSpot + returnYards} yards!");
                play.ElapsedTime += 6.0 + (_rng.NextDouble() * 2.0);
                return;
            }

            // Clamp to valid field position
            fieldPosition = Math.Max(1, Math.Min(99, fieldPosition));

            play.EndFieldPosition = fieldPosition;
            play.Result.LogInformation($"{returner.LastName} returns the kickoff {returnYards} yards to the {fieldPosition}-yard line.");

            // Kickoff return takes 5-8 seconds
            play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
        }
    }
}
