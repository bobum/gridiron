using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
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

                play.Result.LogInformation($"{qb.LastName} complete to {receiver.LastName} for {totalYards} yards!");
            }
            else
            {
                // Incomplete pass
                play.Result.LogInformation($"{qb.LastName} pass incomplete, intended for {receiver.LastName}.");
            }

            // Create the pass segment
            var segment = new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = isComplete,
                Type = passType,
                AirYards = isComplete ? airYards : 0,
                YardsAfterCatch = yardsAfterCatch,
                EndedInFumble = false // Fumble check happens later in FumbleReturn state
            };

            play.PassSegments.Add(segment);
            play.YardsGained = totalYards;

            // Update elapsed time (pass plays take 4-7 seconds - slightly faster than runs)
            play.ElapsedTime += 4.0 + (_rng.NextDouble() * 3.0);
        }

        private void ExecuteSack(Game game, PassPlay play, Player qb)
        {
            // Calculate sack yardage loss (2-10 yards typically)
            var sackYards = -1 * (_rng.Next(2, 11));

            // Don't go past own goal line
            var maxLoss = -1 * game.FieldPosition;
            sackYards = Math.Max(sackYards, maxLoss);

            play.YardsGained = sackYards;

            // Log the sack
            var sacker = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.LB || p.Position == Positions.OLB)
                .OrderByDescending(p => p.Speed + p.Strength)
                .FirstOrDefault();

            if (sacker != null)
            {
                play.Result.LogInformation($"SACK! {sacker.LastName} brings down {qb.LastName} for a loss of {Math.Abs(sackYards)} yards!");
            }
            else
            {
                play.Result.LogInformation($"SACK! {qb.LastName} is brought down for a loss of {Math.Abs(sackYards)} yards!");
            }

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

            // Sacks take less time (2-4 seconds)
            play.ElapsedTime += 2.0 + (_rng.NextDouble() * 2.0);
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
            var yardsToGoal = 100 - fieldPosition;

            return passType switch
            {
                PassType.Screen => _rng.Next(-3, 3),      // Behind or at line of scrimmage
                PassType.Short => _rng.Next(3, 12),       // 3-11 yards
                PassType.Forward => _rng.Next(8, 20),     // 8-19 yards
                PassType.Deep => _rng.Next(18, Math.Min(45, yardsToGoal)), // 18-44 yards (or to goal)
                _ => _rng.Next(5, 15)
            };
        }

        private int CalculateYardsAfterCatch(Game game, Player receiver, int airYards)
        {
            // Check for YAC opportunity
            var yacCheck = new YardsAfterCatchSkillsCheck(_rng, receiver);
            yacCheck.Execute(game);

            if (!yacCheck.Occurred)
            {
                // Tackled immediately (0-2 yards)
                return _rng.Next(0, 3);
            }

            // Good YAC opportunity - receiver breaks tackles
            var yacPotential = (receiver.Speed + receiver.Agility + receiver.Rushing) / 3.0;
            var baseYAC = 3.0 + (yacPotential / 20.0); // 3-8 yards typically

            // Add randomness
            var randomFactor = (_rng.NextDouble() * 8) - 2; // -2 to +6
            var totalYAC = Math.Max(0, (int)Math.Round(baseYAC + randomFactor));

            // 5% chance for big play after catch if receiver is fast
            if (_rng.NextDouble() < 0.05 && receiver.Speed > 85)
            {
                totalYAC += _rng.Next(10, 30);
                game.CurrentPlay.Result.LogInformation($"{receiver.LastName} breaks free! Great run after catch!");
            }

            return totalYAC;
        }
    }
}
