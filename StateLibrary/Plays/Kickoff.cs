using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Configuration;
using StateLibrary.Interfaces;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;
using System;
using System.Collections.Generic;
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

            // Probability for onside attempt when trailing significantly
            return scoreDifferential <= -7 && _rng.NextDouble() < GameProbabilities.Kickoffs.ONSIDE_ATTEMPT_PROBABILITY;
        }

        private void ExecuteOnsideKick(Game game, KickoffPlay play, Player kicker)
        {
            // Onside kicks travel 10-15 yards minimum
            play.KickDistance = 10 + (int)(_rng.NextDouble() * 5);

            play.Result.LogInformation($"{kicker.LastName} attempts an ONSIDE KICK!");

            // Recovery probability based on kicker skill and defense
            var recoveryProb = GameProbabilities.Kickoffs.ONSIDE_RECOVERY_BASE_PROBABILITY
                + (kicker.Kicking / GameProbabilities.Kickoffs.ONSIDE_RECOVERY_SKILL_DENOMINATOR)
                    * GameProbabilities.Kickoffs.ONSIDE_RECOVERY_SKILL_BONUS;

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
            play.ClockStopped = true; // Clock stops after onside kick
        }

        private void ExecuteNormalKickoff(Game game, KickoffPlay play, Player kicker)
        {
            // Calculate kick distance based on kicker skill
            var kickDistanceCheck = new KickoffDistanceSkillsCheckResult(_rng, kicker);
            kickDistanceCheck.Execute(game);

            play.KickDistance = (int)kickDistanceCheck.Result;

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
                play.ClockStopped = true; // Clock stops after kickoff
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
                play.ClockStopped = true; // Clock stops after kickoff
                return;
            }

            // Normal return
            ExecuteKickoffReturn(game, play, landingSpot);
            
            play.ClockStopped = true; // Clock stops after kickoff return
        }

        private bool CheckOutOfBounds(int landingSpot)
        {
            // Kicks between 30-70 yards have small chance of going out of bounds
            if (landingSpot < GameProbabilities.Kickoffs.KICKOFF_OOB_DANGER_MIN
                || landingSpot > GameProbabilities.Kickoffs.KICKOFF_OOB_DANGER_MAX)
            {
                return _rng.NextDouble() < GameProbabilities.Kickoffs.KICKOFF_OOB_NORMAL;
            }

            return _rng.NextDouble() < GameProbabilities.Kickoffs.KICKOFF_OOB_DANGER_ZONE;
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

            // Estimate hang time based on kick distance
            // Typical kickoff hang times: 3.5-5.0 seconds
            // Formula: base + distance factor
            var estimatedHangTime = 3.5 + (play.KickDistance / 20.0);

            // Check for muffed catch (similar to punt muffs)
            var muffChance = GameProbabilities.Kickoffs.KICKOFF_MUFF_BASE;

            // Higher chance on short/line-drive kicks
            if (landingSpot < GameProbabilities.Kickoffs.KICKOFF_MUFF_SHORT_THRESHOLD)
                muffChance = GameProbabilities.Kickoffs.KICKOFF_MUFF_SHORT_KICK;

            // Returner skill factor
            var returnerSkill = (returner.Awareness + returner.Agility) / 2.0;
            var skillFactor = 1.0 - (returnerSkill / GameProbabilities.Kickoffs.KICKOFF_MUFF_SKILL_DENOMINATOR);
            muffChance *= skillFactor;

            var muffed = _rng.NextDouble() < muffChance;

            if (muffed)
            {
                play.MuffedCatch = true;
                HandleMuffedKickoff(game, play, returner, landingSpot);
                return;
            }

            // Check if returner signals for fair catch
            var fairCatchCheck = new FairCatchOccurredSkillsCheck(_rng, estimatedHangTime, landingSpot);
            fairCatchCheck.Execute(game);

            if (fairCatchCheck.Occurred)
            {
                play.FairCatch = true;
                play.PossessionChange = true;

                // Calculate field position from receiving team's perspective
                var fairCatchFieldPosition = 100 - landingSpot;
                play.EndFieldPosition = fairCatchFieldPosition;
                play.YardsGained = landingSpot - 35; // Net yards from kickoff spot

                play.Result.LogInformation($"{returner.LastName} signals and makes the fair catch at the {fairCatchFieldPosition}-yard line.");
                play.ElapsedTime += estimatedHangTime + 0.5;
                return; // No return attempt
            }

            // Calculate return yardage
            var returnCheck = new KickoffReturnYardsSkillsCheckResult(_rng, returner);
            returnCheck.Execute(game);

            var returnYards = (int)returnCheck.Result;

            // Check for blocking penalties during return (illegal blocks, blocks in the back)
            var returnBlockers = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.WR || p.Position == Positions.RB ||
                           p.Position == Positions.CB || p.Position == Positions.S)
                .ToList();
            var coverageTeam = play.OffensePlayersOnField
                .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                           p.Position == Positions.LB || p.Position == Positions.WR)
                .ToList();

            var blockingPenaltyCheck = new BlockingPenaltyOccurredSkillsCheck(
                _rng, returnBlockers, coverageTeam, PlayType.Kickoff);
            blockingPenaltyCheck.Execute(game);

            if (blockingPenaltyCheck.Occurred)
            {
                CheckAndAddPenalty(game, play, blockingPenaltyCheck.PenaltyThatOccurred,
                    PenaltyOccuredWhen.During, play.OffensePlayersOnField, play.DefensePlayersOnField);
            }

            // Create return segment
            var segment = new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = returnYards,
                EndedInFumble = false
            };

            play.ReturnSegments.Add(segment);

            // Calculate field position after return (from receiving team's perspective)
            var fieldPosition = 100 - landingSpot + returnYards;

            // Check for fumble during return
            var fumbleCheck = new FumbleOccurredSkillsCheck(
                _rng,
                returner,
                play.OffensePlayersOnField, // Coverage team (kicking team)
                PlayType.Kickoff,
                false);
            fumbleCheck.Execute(game);

            if (fumbleCheck.Occurred)
            {
                segment.EndedInFumble = true;
                HandleKickoffFumbleRecovery(game, play, returner, fieldPosition);
                return; // Fumble ends the return
            }

            // Check for safety (returner tackled in own end zone)
            if (fieldPosition <= 0)
            {
                play.IsSafety = true;
                play.EndFieldPosition = 0;
                play.Result.LogInformation($"{returner.LastName} is tackled in the end zone! SAFETY!");
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 2.0);
                return;
            }

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

            // Check for tackle penalties on the returner
            if (returnYards > 0)
            {
                var tacklers = play.OffensePlayersOnField
                    .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                               p.Position == Positions.LB || p.Position == Positions.WR)
                    .OrderByDescending(p => p.Speed + p.Tackling)
                    .Take(2)
                    .ToList();

                var tacklePenaltyCheck = new TacklePenaltyOccurredSkillsCheck(
                    _rng, returner, tacklers, TackleContext.Returner);
                tacklePenaltyCheck.Execute(game);

                if (tacklePenaltyCheck.Occurred)
                {
                    CheckAndAddPenalty(game, play, tacklePenaltyCheck.PenaltyThatOccurred,
                        PenaltyOccuredWhen.During, play.OffensePlayersOnField, play.DefensePlayersOnField);
                }

                // **INJURY CHECK: Returner after being tackled**
                var defendersInvolved = tacklers.Count;
                var isBigPlay = returnYards >= 20;
                var isOutOfBounds = fieldPosition <= 0 || fieldPosition >= 100;

                CheckForInjury(game, play, returner, defendersInvolved, isOutOfBounds, isBigPlay, false);

                // **INJURY CHECK: Tacklers (lower risk)**
                foreach (var tackler in tacklers)
                {
                    // Defenders have 50% reduced injury rate
                    if (_rng.NextDouble() < 0.5)
                    {
                        CheckForInjury(game, play, tackler, 1, isOutOfBounds, isBigPlay, false);
                    }
                }
            }

            play.EndFieldPosition = fieldPosition;
            play.Result.LogInformation($"{returner.LastName} returns the kickoff {returnYards} yards to the {fieldPosition}-yard line.");

            // Kickoff return takes 5-8 seconds
            play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
        }

        private void HandleKickoffFumbleRecovery(Game game, KickoffPlay play, Player fumbler, int fumbleSpot)
        {
            // Calculate fumble recovery
            var recoveryCheck = new FumbleRecoverySkillsCheckResult(
                _rng,
                fumbler,
                play.DefensePlayersOnField, // Receiving team
                play.OffensePlayersOnField, // Kicking team (coverage)
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
                // Ball OOB - receiving team (defense) keeps possession at fumble spot
                fumble.RecoveredBy = fumbler;
                fumble.RecoverySpot = fumbleSpot;
                fumble.ReturnYards = 0;

                play.Result.LogInformation($"{fumbler.LastName} fumbles! Ball goes out of bounds. Receiving team retains possession.");
                play.EndFieldPosition = fumbleSpot;
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
            }
            else if (recovery.RecoveredBy != null)
            {
                fumble.RecoveredBy = recovery.RecoveredBy;
                fumble.RecoverySpot = recovery.RecoverySpot;
                fumble.ReturnYards = recovery.ReturnYards;

                // Check if kicking team (offense) recovered
                var kickingTeamRecovered = play.OffensePlayersOnField.Contains(recovery.RecoveredBy);

                if (kickingTeamRecovered)
                {
                    // Kicking team recovered - possession doesn't change (they retain)
                    var finalPosition = fumbleSpot + recovery.ReturnYards;

                    // Check for TD
                    if (finalPosition >= 100)
                    {
                        fumble.RecoveryTouchdown = true;
                        play.IsTouchdown = true;
                        play.EndFieldPosition = 100;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} picks it up and takes it ALL THE WAY for a TOUCHDOWN!");
                    }
                    // Check for safety (recovered in kicking team's end zone - very rare)
                    else if (finalPosition <= 0)
                    {
                        play.IsSafety = true;
                        play.EndFieldPosition = 0;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} recovers in the end zone! SAFETY!");
                    }
                    else
                    {
                        play.EndFieldPosition = finalPosition;
                        play.Result.LogInformation($"{fumbler.LastName} FUMBLES! {recovery.RecoveredBy.LastName} recovers for the kicking team and returns it {Math.Abs(recovery.ReturnYards)} yards!");
                    }

                    play.PossessionChange = false; // Kicking team retains possession
                    play.ElapsedTime += 5.0 + (_rng.NextDouble() * 4.0);
                }
                else
                {
                    // Receiving team recovered their own fumble
                    var finalPosition = fumbleSpot + recovery.ReturnYards;

                    // Check for safety (recovered in own end zone)
                    if (finalPosition <= 0)
                    {
                        play.IsSafety = true;
                        play.EndFieldPosition = 0;
                        play.Result.LogInformation($"{fumbler.LastName} fumbles! {recovery.RecoveredBy.LastName} recovers in the end zone! SAFETY!");
                    }
                    // Check for TD (very rare - recovering own fumble for TD)
                    else if (finalPosition >= 100)
                    {
                        play.IsTouchdown = true;
                        play.EndFieldPosition = 100;
                        play.Result.LogInformation($"{fumbler.LastName} fumbles! {recovery.RecoveredBy.LastName} recovers and somehow takes it to the house! TOUCHDOWN!");
                    }
                    else
                    {
                        play.EndFieldPosition = finalPosition;
                        play.Result.LogInformation($"{fumbler.LastName} fumbles! {recovery.RecoveredBy.LastName} recovers for the receiving team.");
                    }

                    play.PossessionChange = true; // Receiving team gets possession
                    play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
                }
            }

            play.Fumbles.Add(fumble);
        }

        private void HandleMuffedKickoff(Game game, KickoffPlay play, Player returner, int landingSpot)
        {
            play.Result.LogInformation($"The kick is muffed by {returner.LastName}!");

            // Determine who recovers the muff
            // Receiving team more likely to recover - similar to punt muffs
            var receivingTeamRecoveryChance = GameProbabilities.Kickoffs.KICKOFF_MUFF_RECEIVING_TEAM_RECOVERY;
            var receivingTeamRecovers = _rng.NextDouble() < receivingTeamRecoveryChance;

            if (receivingTeamRecovers)
            {
                // Receiving team recovers their own muff
                var recoverer = play.DefensePlayersOnField
                    .OrderByDescending(p => p.Speed + p.Awareness)
                    .FirstOrDefault() ?? returner;

                play.RecoveredBy = recoverer;

                // Usually lose yards on muffed recovery
                var recoveryYards = -5 + (int)(_rng.NextDouble() * 10); // -5 to +5 yards from muff spot
                var fieldPosition = 100 - landingSpot + recoveryYards;

                // Clamp to field boundaries
                fieldPosition = Math.Max(1, Math.Min(99, fieldPosition));

                play.EndFieldPosition = fieldPosition;
                play.PossessionChange = true; // Receiving team gets possession

                play.Result.LogInformation($"{recoverer.LastName} manages to fall on it! Receiving team keeps possession at the {fieldPosition}-yard line.");
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 2.0);
            }
            else
            {
                // Kicking team recovers!
                var recoverer = play.OffensePlayersOnField
                    .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                               p.Position == Positions.LB || p.Position == Positions.WR)
                    .OrderByDescending(p => p.Speed)
                    .FirstOrDefault() ?? play.OffensePlayersOnField.First();

                play.RecoveredBy = recoverer;

                // Ball spotted where recovered
                var fieldPosition = 100 - landingSpot;
                fieldPosition = Math.Max(1, Math.Min(99, fieldPosition));

                play.EndFieldPosition = fieldPosition;
                play.PossessionChange = false; // Kicking team retains possession

                play.Result.LogInformation($"{recoverer.LastName} recovers for the kicking team! Great special teams play!");
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 2.0);
            }
        }

        private void CheckAndAddPenalty(
            Game game,
            KickoffPlay play,
            PenaltyNames penaltyName,
            PenaltyOccuredWhen occurredWhen,
            List<Player> homePlayersOnField,
            List<Player> awayPlayersOnField)
        {
            var penaltyEffect = new PenaltyEffectSkillsCheckResult(
                _rng,
                penaltyName,
                occurredWhen,
                homePlayersOnField,
                awayPlayersOnField,
                play.Possession,
                game.FieldPosition
            );
            penaltyEffect.Execute(game);

            if (penaltyEffect.Result != null)
            {
                var penalty = new Penalty
                {
                    Name = penaltyEffect.Result.PenaltyName,
                    CalledOn = penaltyEffect.Result.CalledOn,
                    Player = penaltyEffect.Result.CommittedBy,
                    OccuredWhen = penaltyEffect.Result.OccurredWhen,
                    Yards = penaltyEffect.Result.Yards,
                    Accepted = penaltyEffect.Result.Accepted
                };
                play.Penalties.Add(penalty);

                play.Result.LogInformation($"PENALTY: {penalty.Name} on {penalty.CalledOn}, {penalty.Yards} yards");
            }
        }

        /// <summary>
        /// Checks if a player sustains an injury during the play and handles substitution if needed
        /// </summary>
        private void CheckForInjury(Game game, KickoffPlay play, Player player, int defendersInvolved, bool isOutOfBounds, bool isBigPlay, bool isSack)
        {
            // Skip if player is already injured
            if (player.IsInjured)
                return;

            var injuryCheck = new InjuryOccurredSkillsCheck(
                _rng,
                PlayType.Kickoff,
                player,
                defendersInvolved,
                isOutOfBounds,
                isBigPlay,
                isSack);
            injuryCheck.Execute(game);

            if (injuryCheck.Occurred)
            {
                // Determine injury details
                var injuryEffect = new InjuryEffectSkillsCheckResult(_rng, player, PlayType.Kickoff);
                injuryEffect.Execute(game);

                var injury = new Injury
                {
                    Type = injuryEffect.Result.InjuryType,
                    Severity = injuryEffect.Result.Severity,
                    InjuredPlayer = player,
                    PlayNumber = game.Plays.Count,
                    RemovedFromPlay = injuryEffect.Result.RequiresImmediateRemoval,
                    PlaysUntilReturn = CalculateRecoveryTime(injuryEffect.Result.Severity)
                };

                // Set player's current injury
                player.CurrentInjury = injury;

                // Add to play's injury list
                play.Injuries.Add(injury);

                // Log the injury
                LogInjury(play, injury);

                // If player must be removed immediately, substitute them
                if (injury.RemovedFromPlay)
                {
                    SubstituteInjuredPlayer(game, play, player, injury);
                }
            }
        }

        /// <summary>
        /// Calculates how many plays until player can return based on severity
        /// </summary>
        private int CalculateRecoveryTime(InjurySeverity severity)
        {
            return severity switch
            {
                InjurySeverity.Minor => _rng.Next(InjuryProbabilities.MINOR_INJURY_MIN_PLAYS, InjuryProbabilities.MINOR_INJURY_MAX_PLAYS + 1),
                InjurySeverity.Moderate => InjuryProbabilities.MODERATE_INJURY_PLAYS,
                InjurySeverity.GameEnding => InjuryProbabilities.GAME_ENDING_INJURY_PLAYS,
                _ => 0
            };
        }

        /// <summary>
        /// Logs injury information to the play-by-play
        /// </summary>
        private void LogInjury(KickoffPlay play, Injury injury)
        {
            var injuryTypeText = injury.Type switch
            {
                InjuryType.Ankle => "ankle",
                InjuryType.Knee => "knee",
                InjuryType.Shoulder => "shoulder",
                InjuryType.Concussion => "head",
                InjuryType.Hamstring => "hamstring",
                _ => "injury"
            };

            play.Result.LogInformation($"??  {injury.InjuredPlayer.LastName} is down on the field! Trainers are attending to a {injuryTypeText} injury.");
        }

        /// <summary>
        /// Substitutes an injured player with a backup from the depth chart
        /// </summary>
        private void SubstituteInjuredPlayer(Game game, KickoffPlay play, Player injuredPlayer, Injury injury)
        {
            // Determine if player is on offense or defense
            var isOffense = play.OffensePlayersOnField.Contains(injuredPlayer);
            var playersOnField = isOffense ? play.OffensePlayersOnField : play.DefensePlayersOnField;
            var team = play.Possession == Possession.Home ? game.HomeTeam : game.AwayTeam;
            var depthChart = isOffense ? team.OffenseDepthChart : team.DefenseDepthChart;

            // Find replacement from depth chart
            Player? replacement = null;
            if (depthChart.Chart.TryGetValue(injuredPlayer.Position, out var playersAtPosition))
            {
                // Find first available player who is not injured and not already on field
                replacement = playersAtPosition
                    .FirstOrDefault(p => !p.IsInjured && !playersOnField.Contains(p));
            }

            if (replacement != null)
            {
                // Swap players
                var index = playersOnField.IndexOf(injuredPlayer);
                playersOnField[index] = replacement;
                injury.ReplacementPlayer = replacement;

                play.Result.LogInformation($"{injuredPlayer.LastName} is being helped off the field. {replacement.LastName} enters the game.");
            }
            else
            {
                play.Result.LogWarning($"No replacement available for {injuredPlayer.LastName} at {injuredPlayer.Position}! Team playing short-handed.");
            }
        }
    }
}
