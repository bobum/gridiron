using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
using StateLibrary.SkillsCheckResults;
using System;
using System.Linq;

namespace StateLibrary.Plays
{
    //Punt could be a regular punt,
    //or a fake punt pass
    //or a fake punt run
    //blocked punt
    //a muffed snap
    public sealed class Punt : IGameAction
    {
        private ISeedableRandom _rng;

        public Punt(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game)
        {
            var play = (PuntPlay)game.CurrentPlay;

            // Get the punter
            var punter = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.P);
            if (punter == null)
            {
                // No punter, use kicker or random player
                punter = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.K)
                    ?? play.OffensePlayersOnField.FirstOrDefault();

                if (punter == null)
                {
                    play.Result.LogWarning("No punter found for punt play!");
                    return;
                }
            }

            play.Punter = punter;

            // Get long snapper
            var longSnapper = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.LS)
                ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.C);

            if (longSnapper == null)
            {
                play.Result.LogWarning("No long snapper found for punt!");
                return;
            }

            // Check for bad snap
            var badSnapCheck = new BadSnapOccurredSkillsCheck(_rng, longSnapper);
            badSnapCheck.Execute(game);

            if (badSnapCheck.Occurred)
            {
                ExecuteBadSnap(game, play, punter, longSnapper);
                return;
            }

            // Good snap
            play.GoodSnap = true;

            // Check for blocked punt
            var blockCheck = new PuntBlockOccurredSkillsCheck(_rng);
            blockCheck.Execute(game);

            if (blockCheck.Occurred)
            {
                ExecuteBlockedPunt(game, play, punter);
                return;
            }

            // Execute normal punt
            ExecuteNormalPunt(game, play, punter);
        }

        private void ExecuteBadSnap(Game game, PuntPlay play, Player punter, Player longSnapper)
        {
            play.GoodSnap = false;

            // Calculate yardage lost on bad snap
            var badSnapYardsResult = new BadSnapYardsSkillsCheckResult(_rng, game.FieldPosition);
            badSnapYardsResult.Execute(game);
            var yardsLost = badSnapYardsResult.Result;

            play.YardsGained = yardsLost;

            play.Result.LogInformation($"BAD SNAP! {longSnapper.LastName} snaps it over {punter.LastName}'s head!");

            // Check if it's a safety (ball recovered in own end zone)
            if (game.FieldPosition + yardsLost <= 0)
            {
                play.Result.LogInformation($"The ball rolls into the end zone! SAFETY!");
                play.YardsGained = -1 * game.FieldPosition; // Ball at goal line
                play.IsSafety = true;
            }
            else
            {
                // Punter recovers the bad snap
                play.Result.LogInformation($"{punter.LastName} recovers the loose ball after a loss of {Math.Abs(yardsLost)} yards!");
            }

            // Bad snap plays take 4-8 seconds (chaos)
            play.ElapsedTime += 4.0 + (_rng.NextDouble() * 4.0);
        }

        private void ExecuteBlockedPunt(Game game, PuntPlay play, Player punter)
        {
            play.Blocked = true;

            // Find the blocker (defensive line or rush specialist)
            var blocker = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.LB || p.Position == Positions.OLB)
                .OrderByDescending(p => p.Speed + p.Strength)
                .FirstOrDefault();

            play.BlockedBy = blocker;

            if (blocker != null)
            {
                play.Result.LogInformation($"BLOCKED! {blocker.LastName} gets a hand on it!");
            }
            else
            {
                play.Result.LogInformation($"BLOCKED PUNT!");
            }

            // Determine who recovers the blocked punt
            var offenseRecoveryChance = 0.5; // 50/50
            var offenseRecovers = _rng.NextDouble() < offenseRecoveryChance;

            if (offenseRecovers)
            {
                // Offense recovers
                var recoverer = play.OffensePlayersOnField
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault() ?? punter;

                play.RecoveredBy = recoverer;

                // Calculate recovery yards (usually a loss)
                var recoveryYards = -5 - (int)(_rng.NextDouble() * 5); // -5 to -10 yards
                recoveryYards = Math.Max(-1 * game.FieldPosition, recoveryYards); // Can't go past own goal

                play.RecoveryYards = recoveryYards;
                play.YardsGained = recoveryYards;

                play.Result.LogInformation($"{recoverer.LastName} falls on it for the offense, loss of {Math.Abs(recoveryYards)} yards.");
            }
            else
            {
                // Defense recovers!
                var recoverer = blocker ?? play.DefensePlayersOnField
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault();

                play.RecoveredBy = recoverer;
                play.PossessionChange = true;

                // Calculate where the blocked punt bounced
                // Near goal line: can bounce backward into end zone
                // Midfield: usually bounces forward
                var baseBounce = -10.0 + (_rng.NextDouble() * 25.0); // -10 to +15 yards
                var randomFactor = (_rng.NextDouble() * 10.0) - 5.0; // ±5 yards variance
                var bouncedYards = baseBounce + randomFactor;

                // Calculate final position after bounce and recovery
                var finalPosition = game.FieldPosition + (int)bouncedYards;

                // Check if ball bounced into end zone (defensive TD)
                if (finalPosition <= 0)
                {
                    play.IsTouchdown = true;
                    play.RecoveryYards = (int)bouncedYards;
                    play.YardsGained = (int)bouncedYards;

                    if (recoverer != null)
                    {
                        play.Result.LogInformation($"{recoverer.LastName} recovers the blocked punt in the end zone! TOUCHDOWN!");
                    }
                }
                // Check if defense returns it all the way (ran it to opponent's end zone)
                else if (finalPosition >= 100)
                {
                    play.IsTouchdown = true;
                    var recoveryYards = 100 - game.FieldPosition;
                    play.RecoveryYards = recoveryYards;
                    play.YardsGained = recoveryYards;

                    if (recoverer != null)
                    {
                        play.Result.LogInformation($"{recoverer.LastName} scoops it up and takes it to the house! TOUCHDOWN!");
                    }
                }
                else
                {
                    // Normal recovery and return
                    var recoveryYards = Math.Min((int)bouncedYards, 100 - game.FieldPosition);
                    play.RecoveryYards = recoveryYards;
                    play.YardsGained = recoveryYards;

                    if (recoverer != null)
                    {
                        play.Result.LogInformation($"{recoverer.LastName} recovers the blocked punt and returns it {recoveryYards} yards!");
                    }
                }
            }

            // Blocked punts take 3-6 seconds
            play.ElapsedTime += 3.0 + (_rng.NextDouble() * 3.0);
        }

        private void ExecuteNormalPunt(Game game, PuntPlay play, Player punter)
        {
            // Calculate punt distance
            var distanceResult = new PuntDistanceSkillsCheckResult(_rng, punter, game.FieldPosition);
            distanceResult.Execute(game);
            var puntDistance = distanceResult.Result;

            play.PuntDistance = puntDistance;

            // Calculate hang time
            var hangTimeResult = new PuntHangTimeSkillsCheckResult(_rng, puntDistance);
            hangTimeResult.Execute(game);
            var hangTime = hangTimeResult.Result;

            play.HangTime = hangTime;

            // Calculate where punt lands
            var puntLandingSpot = game.FieldPosition + puntDistance;

            // Check for touchback (into end zone)
            if (puntLandingSpot >= 100)
            {
                play.Touchback = true;
                play.YardsGained = 80 - game.FieldPosition; // Touchback brings to 20 yard line (opponent's perspective)
                play.PossessionChange = true;

                play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. The ball sails into the end zone for a touchback.");
                play.ElapsedTime += hangTime + 0.5;
                return;
            }

            // Check if punt goes out of bounds
            var outOfBoundsCheck = new PuntOutOfBoundsOccurredSkillsCheck(_rng, puntLandingSpot);
            outOfBoundsCheck.Execute(game);

            if (outOfBoundsCheck.Occurred)
            {
                play.OutOfBounds = true;
                play.YardsGained = puntDistance;
                play.PossessionChange = true;

                play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards out of bounds. No return.");
                play.ElapsedTime += hangTime + 0.5;
                return;
            }

            // Check if punt is downed by punting team
            var downedCheck = new PuntDownedOccurredSkillsCheck(_rng, puntLandingSpot, hangTime);
            downedCheck.Execute(game);

            if (downedCheck.Occurred)
            {
                play.Downed = true;
                play.DownedAtYardLine = puntLandingSpot;
                play.YardsGained = puntDistance;
                play.PossessionChange = true;

                var downer = play.OffensePlayersOnField
                    .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                               p.Position == Positions.WR || p.Position == Positions.RB)
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault();

                if (downer != null)
                {
                    play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. {downer.LastName} downs it at the {100 - puntLandingSpot} yard line!");
                }
                else
                {
                    play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. Downed at the {100 - puntLandingSpot} yard line.");
                }

                play.ElapsedTime += hangTime + 1.0;
                return;
            }

            // Punt will be returned
            ExecutePuntReturn(game, play, punter, puntDistance, puntLandingSpot, hangTime);
        }

        private void ExecutePuntReturn(Game game, PuntPlay play, Player punter, int puntDistance, int puntLandingSpot, double hangTime)
        {
            // Get punt returner
            var returner = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.WR || p.Position == Positions.CB ||
                           p.Position == Positions.RB || p.Position == Positions.S)
                .OrderByDescending(p => p.Speed + p.Catching + p.Agility)
                .FirstOrDefault();

            if (returner == null)
            {
                // No returner, treat as downed
                play.Downed = true;
                play.DownedAtYardLine = puntLandingSpot;
                play.YardsGained = puntDistance;
                play.PossessionChange = true;
                play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. No returner back!");
                play.ElapsedTime += hangTime + 1.0;
                return;
            }

            // Check for fair catch
            var fairCatchCheck = new FairCatchOccurredSkillsCheck(_rng, hangTime, puntLandingSpot);
            fairCatchCheck.Execute(game);

            if (fairCatchCheck.Occurred)
            {
                play.FairCatch = true;
                play.YardsGained = puntDistance;
                play.PossessionChange = true;

                play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. {returner.LastName} signals for and makes the fair catch.");
                play.ElapsedTime += hangTime + 0.5;
                return;
            }

            // Check for muffed catch
            var muffCheck = new MuffedCatchOccurredSkillsCheck(_rng, returner, hangTime);
            muffCheck.Execute(game);

            if (muffCheck.Occurred)
            {
                play.MuffedCatch = true;
                play.MuffedBy = returner;

                play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. {returner.LastName} MUFFS THE CATCH!");

                // Determine who recovers the muff
                var defenseRecoveryChance = 0.6; // Receiving team more likely to recover (60%)
                var defenseRecovers = _rng.NextDouble() < defenseRecoveryChance;

                if (defenseRecovers)
                {
                    // Receiving team recovers their own muff
                    var recoverer = returner;
                    play.RecoveredBy = recoverer;

                    // Usually lose yards on muffed recovery
                    var recoveryYards = -5 + (int)(_rng.NextDouble() * 10); // -5 to +5 yards from muff spot
                    var actualReturnYards = puntDistance + recoveryYards;

                    // Clamp to field boundaries
                    var maxReturn = 100 - game.FieldPosition;
                    actualReturnYards = Math.Min(actualReturnYards, maxReturn);

                    play.YardsGained = actualReturnYards;
                    play.PossessionChange = true;

                    play.Result.LogInformation($"{recoverer.LastName} manages to fall on it after a {recoveryYards} yard scramble!");
                }
                else
                {
                    // Punting team recovers!
                    var recoverer = play.OffensePlayersOnField
                        .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                                   p.Position == Positions.LB)
                        .OrderByDescending(p => p.Speed)
                        .FirstOrDefault() ?? play.OffensePlayersOnField.First();

                    play.RecoveredBy = recoverer;
                    play.YardsGained = puntDistance;
                    // Note: No possession change! Punting team keeps the ball

                    play.Result.LogInformation($"{recoverer.LastName} recovers for the punting team! Great special teams play!");
                }

                play.ElapsedTime += hangTime + 2.0 + (_rng.NextDouble() * 2.0);
                return;
            }

            // Clean catch, now calculate return
            play.Result.LogInformation($"{punter.LastName} punts {puntDistance} yards. {returner.LastName} back to receive...");

            var returnYardsResult = new PuntReturnYardsSkillsCheckResult(_rng, returner, hangTime, play.OffensePlayersOnField);
            returnYardsResult.Execute(game);
            var returnYards = returnYardsResult.Result;

            // Clamp to field boundaries
            var yardsToGoal = 100 - puntLandingSpot;
            returnYards = Math.Min(returnYards, yardsToGoal);

            var totalYards = puntDistance + returnYards;

            // Create return segment
            var segment = new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = returnYards,
                EndedInFumble = false // Fumble check would happen later in FumbleReturn state
            };

            play.ReturnSegments.Add(segment);
            play.YardsGained = totalYards;
            play.PossessionChange = true;

            // Log return narrative
            if (returnYards <= 0)
            {
                play.Result.LogInformation($"{returner.LastName} is immediately tackled for no return!");
            }
            else if (returnYards <= 5)
            {
                play.Result.LogInformation($"{returner.LastName} returns it for {returnYards} yards.");
            }
            else if (returnYards <= 15)
            {
                play.Result.LogInformation($"{returner.LastName} finds some room and returns it {returnYards} yards!");
            }
            else if (returnYards < yardsToGoal)
            {
                play.Result.LogInformation($"Great return by {returner.LastName}! Brings it back {returnYards} yards!");
            }
            else
            {
                play.Result.LogInformation($"HE'S GOT A LANE! {returner.LastName} takes it {returnYards} yards to the house! TOUCHDOWN!");
                play.IsTouchdown = true;
            }

            // Punt returns take hang time + return time (2-6 seconds for return)
            play.ElapsedTime += hangTime + 2.0 + (_rng.NextDouble() * 4.0);
        }
    }
}
