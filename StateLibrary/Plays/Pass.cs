using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
using StateLibrary.SkillsCheckResults;
using System.Linq;

namespace StateLibrary.Plays
{
    //Pass plays can be your typical downfield pass play
    //a lateral
    //a halfback pass
    //a fake punt would be in the Punt class - those could be run or pass...
    //a fake fieldgoal would be in the FieldGoal class - those could be run or pass...
    //a muffed snap on a punt would be in the Punt class - those could be run or pass...
    //a muffed snap on a fieldgoald would be in the FieldGoal class - those could be run or pass...
    public sealed class Pass : IGameAction
    {
        private ISeedableRandom _rng;

        public Pass(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game)
        {
            var play = (PassPlay)game.CurrentPlay;

            // Get the QB
            var qb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
            if (qb == null)
            {
                play.Result.LogWarning("No quarterback found for pass play!");
                return;
            }

            // Check pass protection (sack check)
            var protectionCheck = new PassProtectionSkillsCheck(_rng);
            protectionCheck.Execute(game);

            if (!protectionCheck.Occurred)
            {
                // QB is sacked!
                ExecuteSack(game, play, qb);
                return;
            }

            // Protection holds, check for QB pressure
            var pressureCheck = new QBPressureSkillsCheck(_rng);
            pressureCheck.Execute(game);
            var underPressure = pressureCheck.Occurred;

            if (underPressure)
            {
                play.Result.LogInformation($"{qb.LastName} is under pressure!");
            }

            // Select target receiver
            var receiver = SelectTargetReceiver(play);
            if (receiver == null)
            {
                play.Result.LogWarning("No receivers available!");
                return;
            }

            // Determine pass type and air yards
            var passType = DeterminePassType();
            var airYards = CalculateAirYards(passType, game.FieldPosition);

            // Check if pass is completed
            var completionCheck = new PassCompletionSkillsCheck(_rng, qb, receiver, underPressure);
            completionCheck.Execute(game);

            var isComplete = completionCheck.Occurred;
            var yardsAfterCatch = 0;
            var totalYards = 0;

            if (isComplete)
            {
                // Pass completed - calculate yards after catch
                yardsAfterCatch = CalculateYardsAfterCatch(game, receiver, airYards);
                totalYards = airYards + yardsAfterCatch;

                // Ensure we don't exceed field boundaries
                var yardsToGoal = 100 - game.FieldPosition;
                totalYards = Math.Min(totalYards, yardsToGoal);

                // Create the pass segment
                var segment = new PassSegment
                {
                    Passer = qb,
                    Receiver = receiver,
                    IsComplete = isComplete,
                    Type = passType,
                    AirYards = airYards,
                    YardsAfterCatch = yardsAfterCatch,
                    EndedInFumble = false
                };

                play.PassSegments.Add(segment);
                play.YardsGained = totalYards;

                // Calculate current field position after the catch
                var currentFieldPosition = game.FieldPosition + totalYards;

                // Check for fumble after the catch (similar probability to run plays)
                var fumbleCheck = new FumbleOccurredSkillsCheck(
                    _rng,
                    receiver,
                    play.DefensePlayersOnField,
                    PlayType.Pass,
                    isQBSack: false);
                fumbleCheck.Execute(game);

                if (fumbleCheck.Occurred)
                {
                    segment.EndedInFumble = true;
                    play.Result.LogInformation($"{qb.LastName} complete to {receiver.LastName} for {totalYards} yards!");
                    HandleFumbleRecovery(game, play, receiver, currentFieldPosition);
                }
                else
                {
                    play.Result.LogInformation($"{qb.LastName} complete to {receiver.LastName} for {totalYards} yards!");
                    // Update elapsed time (pass plays take 4-7 seconds - slightly faster than runs)
                    play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
                }
            }
            else
            {
                // Incomplete pass - check for interception
                var interceptionCheck = new InterceptionOccurredSkillsCheck(_rng, qb, receiver, underPressure);
                interceptionCheck.Execute(game);

                if (interceptionCheck.Occurred)
                {
                    // INTERCEPTION!
                    var interceptionSpot = game.FieldPosition + airYards;
                    // Clamp to field boundaries
                    interceptionSpot = Math.Max(0, Math.Min(100, interceptionSpot));

                    // Use SkillsCheckResult to handle full interception scenario
                    var interceptionResult = new InterceptionSkillsCheckResult(
                        _rng,
                        qb,
                        receiver,
                        play.OffensePlayersOnField,
                        play.DefensePlayersOnField,
                        interceptionSpot);
                    interceptionResult.Execute(game);

                    var result = interceptionResult.Result;

                    // Log interception
                    play.Result.LogInformation($"INTERCEPTION! {result.Interceptor.LastName} picks off {qb.LastName}!");

                    if (result.IsPickSix)
                    {
                        play.IsTouchdown = true;
                        play.YardsGained = -1 * play.StartFieldPosition; // Defense scores
                        play.Result.LogInformation($"{result.Interceptor.LastName} takes it ALL THE WAY! PICK-SIX TOUCHDOWN!");
                    }
                    else if (result.FumbledDuringReturn)
                    {
                        play.Result.LogInformation($"{result.Interceptor.LastName} returns it {result.ReturnYards} yards but fumbles!");

                        // Handle fumble recovery logging
                        if (result.FumbleRecovery.OutOfBounds)
                        {
                            play.Result.LogInformation($"Fumble goes out of bounds at the {result.FinalPosition} yard line.");
                        }
                        else
                        {
                            play.Result.LogInformation($"{result.FumbleRecovery.RecoveredBy.LastName} recovers the fumble!");
                        }

                        play.YardsGained = result.FinalPosition - play.StartFieldPosition;
                    }
                    else
                    {
                        // Normal interception return
                        play.YardsGained = result.FinalPosition - play.StartFieldPosition;
                        if (result.ReturnYards > 0)
                        {
                            play.Result.LogInformation($"{result.Interceptor.LastName} returns it {result.ReturnYards} yards!");
                        }
                    }

                    // Create the Interception domain object
                    var interception = new DomainObjects.Interception
                    {
                        InterceptedBy = result.Interceptor,
                        ThrownBy = qb,
                        InterceptionYardLine = interceptionSpot,
                        ReturnYards = result.ReturnYards,
                        FumbledDuringReturn = result.FumbledDuringReturn,
                        RecoveredBy = result.FumbledDuringReturn ? result.FumbleRecovery?.RecoveredBy : null
                    };

                    // Create pass segment for the intercepted pass
                    var segment = new PassSegment
                    {
                        Passer = qb,
                        Receiver = receiver,
                        IsComplete = false,
                        Type = passType,
                        AirYards = 0,
                        YardsAfterCatch = 0,
                        EndedInFumble = false
                    };

                    play.PassSegments.Add(segment);

                    // Mark interception and possession change
                    play.Interception = true;
                    play.InterceptionDetails = interception;
                    play.PossessionChange = result.PossessionChange;

                    // Handle fumbles during return
                    if (result.FumbledDuringReturn)
                    {
                        var fumble = new Fumble
                        {
                            FumbledBy = result.Interceptor,
                            RecoveredBy = result.FumbleRecovery?.RecoveredBy,
                            FumbleYardLine = interceptionSpot - result.ReturnYards,
                            OutOfBounds = result.FumbleRecovery?.OutOfBounds ?? false
                        };
                        play.Fumbles.Add(fumble);

                        // Check if offense recovered - they get possession back
                        if (result.FumbleRecovery?.RecoveredBy != null &&
                            play.OffensePlayersOnField.Contains(result.FumbleRecovery.RecoveredBy))
                        {
                            play.PossessionChange = false; // Offense gets ball back
                        }
                    }

                    // Update elapsed time
                    play.ElapsedTime += 4.0 + (_rng.NextDouble() * 4.0);
                }
                else
                {
                    // Incomplete pass (not intercepted)
                    play.Result.LogInformation($"{qb.LastName} pass incomplete, intended for {receiver.LastName}.");

                    // Create the pass segment for incomplete
                    var segment = new PassSegment
                    {
                        Passer = qb,
                        Receiver = receiver,
                        IsComplete = isComplete,
                        Type = passType,
                        AirYards = 0,
                        YardsAfterCatch = yardsAfterCatch,
                        EndedInFumble = false
                    };

                    play.PassSegments.Add(segment);
                    play.YardsGained = totalYards;

                    // Update elapsed time (pass plays take 4-7 seconds - slightly faster than runs)
                    play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
                }
            }
        }

        private void ExecuteSack(Game game, PassPlay play, Player qb)
        {
            // Calculate sack yardage loss using SkillsCheckResult
            var sackYardsResult = new SackYardsSkillsCheckResult(_rng, game.FieldPosition);
            sackYardsResult.Execute(game);
            var sackYards = sackYardsResult.Result;

            play.YardsGained = sackYards;

            // Get the sacker for fumble check
            var sackers = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.LB || p.Position == Positions.OLB)
                .OrderByDescending(p => p.Speed + p.Strength)
                .Take(2) // Get top 2 defenders for gang tackle scenario
                .ToList();

            var sacker = sackers.FirstOrDefault();

            // Calculate current field position after sack
            var currentFieldPosition = game.FieldPosition + sackYards;

            // Check for fumble on the sack (strip sack) - higher probability than normal run
            var fumbleCheck = new FumbleOccurredSkillsCheck(
                _rng,
                qb,
                sackers,
                PlayType.Pass,
                isQBSack: true); // This triggers the 12% fumble rate
            fumbleCheck.Execute(game);

            // Create incomplete pass segment for sack
            var dummyReceiver = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.WR)
                               ?? play.OffensePlayersOnField.First();

            var segment = new PassSegment
            {
                Passer = qb,
                Receiver = dummyReceiver,
                IsComplete = false,
                Type = PassType.Short,
                AirYards = 0,
                YardsAfterCatch = 0,
                EndedInFumble = false
            };

            play.PassSegments.Add(segment);

            if (fumbleCheck.Occurred)
            {
                segment.EndedInFumble = true;
                if (sacker != null)
                {
                    play.Result.LogInformation($"SACK and STRIP! {sacker.LastName} brings down {qb.LastName}!");
                }
                else
                {
                    play.Result.LogInformation($"SACK! {qb.LastName} is brought down and fumbles!");
                }
                HandleFumbleRecovery(game, play, qb, currentFieldPosition);
            }
            else
            {
                // No fumble - log normal sack
                if (sacker != null)
                {
                    play.Result.LogInformation($"SACK! {sacker.LastName} brings down {qb.LastName} for a loss of {Math.Abs(sackYards)} yards!");
                }
                else
                {
                    play.Result.LogInformation($"SACK! {qb.LastName} is brought down for a loss of {Math.Abs(sackYards)} yards!");
                }

                // Sacks take less time (2-4 seconds)
                play.ElapsedTime += 2.0 + (_rng.NextDouble() * 2.0);
            }
        }

        private Player? SelectTargetReceiver(PassPlay play)
        {
            // Get all eligible receivers (WR, TE, RB)
            var receivers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.WR ||
                p.Position == Positions.TE ||
                p.Position == Positions.RB).ToList();

            if (!receivers.Any())
                return null;

            // Weight selection by catching ability
            var totalCatching = receivers.Sum(r => r.Catching);
            if (totalCatching == 0)
                return receivers[_rng.Next(receivers.Count)];

            var randomValue = _rng.NextDouble() * totalCatching;
            var cumulative = 0.0;

            foreach (var receiver in receivers)
            {
                cumulative += receiver.Catching;
                if (randomValue <= cumulative)
                    return receiver;
            }

            return receivers.Last();
        }

        private PassType DeterminePassType()
        {
            var random = _rng.NextDouble();

            if (random < 0.15) return PassType.Screen;  // 15%
            if (random < 0.50) return PassType.Short;   // 35%
            if (random < 0.85) return PassType.Forward; // 35%
            return PassType.Deep;                        // 15%
        }

        private int CalculateAirYards(PassType passType, int fieldPosition)
        {
            var airYardsResult = new AirYardsSkillsCheckResult(_rng, passType, fieldPosition);
            airYardsResult.Execute(null!); // Game not needed for this calculation
            return airYardsResult.Result;
        }

        private int CalculateYardsAfterCatch(Game game, Player receiver, int airYards)
        {
            var yacResult = new YardsAfterCatchSkillsCheckResult(_rng, receiver);
            yacResult.Execute(game);
            return yacResult.Result;
        }

        private void HandleFumbleRecovery(Game game, PassPlay play, Player fumbler, int fumbleSpot)
        {
            // Calculate fumble recovery
            var recoveryCheck = new FumbleRecoverySkillsCheckResult(
                _rng,
                fumbler,
                play.OffensePlayersOnField,
                play.DefensePlayersOnField,
                fumbleSpot);
            recoveryCheck.Execute(game);

            var recovery = recoveryCheck.Result;

            // Create fumble record
            var fumble = new DomainObjects.Fumble
            {
                FumbledBy = fumbler,
                FumbleSpot = fumbleSpot,
                OutOfBounds = recovery.OutOfBounds
            };

            if (recovery.OutOfBounds)
            {
                // Ball OOB - offense keeps possession at fumble spot
                fumble.RecoveredBy = fumbler; // Technically not recovered, but offense retains
                fumble.RecoverySpot = fumbleSpot;
                fumble.ReturnYards = 0;

                play.Result.LogInformation($"{fumbler.LastName} fumbles! Ball goes out of bounds. {play.Possession} retains possession.");
                play.YardsGained = fumbleSpot - play.StartFieldPosition;
                play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
            }
            else if (recovery.RecoveredBy != null)
            {
                fumble.RecoveredBy = recovery.RecoveredBy;
                fumble.RecoverySpot = recovery.RecoverySpot;
                fumble.ReturnYards = recovery.ReturnYards;

                // Determine if defense recovered
                var defenseRecovered = play.DefensePlayersOnField.Contains(recovery.RecoveredBy);

                if (defenseRecovered)
                {
                    // Defense recovered
                    var finalPosition = fumbleSpot + recovery.ReturnYards;

                    // Check for TD
                    if (finalPosition >= 100)
                    {
                        fumble.RecoveryTouchdown = true;
                        play.IsTouchdown = true;
                        play.YardsGained = 100 - play.StartFieldPosition;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} picks it up and takes it ALL THE WAY for a TOUCHDOWN!");
                    }
                    // Check for safety (recovered in fumbling team's end zone)
                    else if (finalPosition <= 0)
                    {
                        play.IsSafety = true;
                        play.YardsGained = -1 * play.StartFieldPosition;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} recovers in the end zone! SAFETY!");
                    }
                    else
                    {
                        play.YardsGained = finalPosition - play.StartFieldPosition;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} recovers and returns it {Math.Abs(recovery.ReturnYards)} yards!");
                    }

                    play.PossessionChange = true;
                    play.ElapsedTime += 4.0 + (_rng.NextDouble() * 4.0);
                }
                else
                {
                    // Offense recovered
                    var finalPosition = fumbleSpot + recovery.ReturnYards;

                    // Check for safety (recovered in own end zone)
                    if (finalPosition <= 0)
                    {
                        play.IsSafety = true;
                        play.YardsGained = -1 * play.StartFieldPosition;
                        play.Result.LogInformation($"{fumbler.LastName} fumbles! {recovery.RecoveredBy.LastName} recovers in the end zone! SAFETY!");
                    }
                    else
                    {
                        play.YardsGained = finalPosition - play.StartFieldPosition;
                        play.Result.LogInformation($"{fumbler.LastName} fumbles! {recovery.RecoveredBy.LastName} recovers for the offense.");
                    }

                    play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
                }
            }

            play.Fumbles.Add(fumble);
        }

    }
}
