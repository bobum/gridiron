using DomainObjects;
using DomainObjects.Helpers;
using DomainObjects.Penalties;
using Microsoft.Extensions.Logging;
using StateLibrary.Configuration;
using StateLibrary.Interfaces;
using StateLibrary.SkillsChecks;
using StateLibrary.SkillsCheckResults;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.Plays
{
    //Run plays can be your typical, hand it off to the guy play
    //or a QB scramble
    //or a 2-pt conversion
    //or a kneel
    //a fake punt would be in the Punt class - those could be run or pass...
    //a muffed snap
    public sealed class Run : IGameAction
    {
        private ISeedableRandom _rng;

        public Run(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game)
        {
            var play = (RunPlay)game.CurrentPlay;

            // Determine the ball carrier (RB or QB for scramble)
            var ballCarrier = DetermineBallCarrier(play);

            if (ballCarrier == null)
            {
                play.Result.LogWarning("No ball carrier found for run play!");
                return;
            }

            // Determine run direction
            var direction = DetermineRunDirection();

            // Check if offensive line creates a good running lane
            var blockingCheck = new BlockingSuccessSkillsCheck(_rng);
            blockingCheck.Execute(game);

            var blockingSuccess = blockingCheck.Occurred;
            var blockingModifier = blockingSuccess ? 1.2 : 0.8; // +20% or -20% yards

            // Check for blocking penalties during run blocking
            var offensiveLine = play.OffensePlayersOnField
                .Where(p => p.Position == Positions.T || p.Position == Positions.G || p.Position == Positions.C)
                .ToList();
            var defensiveLine = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.LB || p.Position == Positions.OLB)
                .ToList();

            var blockingPenaltyCheck = new BlockingPenaltyOccurredSkillsCheck(
                _rng, offensiveLine, defensiveLine, PlayType.Run);
            blockingPenaltyCheck.Execute(game);

            if (blockingPenaltyCheck.Occurred)
            {
                // Use new IPenalty architecture if available, otherwise fall back to old system
                if (blockingPenaltyCheck.PenaltyInstance != null)
                {
                    CheckAndAddPenaltyInstance(game, play, blockingPenaltyCheck.PenaltyInstance,
                        PenaltyOccuredWhen.During, play.OffensePlayersOnField, play.DefensePlayersOnField);
                }
                else
                {
                    CheckAndAddPenalty(game, play, blockingPenaltyCheck.PenaltyThatOccurred,
                        PenaltyOccuredWhen.During, play.OffensePlayersOnField, play.DefensePlayersOnField);
                }
            }

            // Calculate base yardage using SkillsCheckResult
            var runYardsResult = new RunYardsSkillsCheckResult(_rng, ballCarrier, play.OffensePlayersOnField, play.DefensePlayersOnField);
            runYardsResult.Execute(game);
            var baseYards = runYardsResult.Result;
            var adjustedYards = (int)(baseYards * blockingModifier);

            // Check for tackle break (adds 3-8 yards)
            var tackleBreakCheck = new TackleBreakSkillsCheck(_rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            if (tackleBreakCheck.Occurred)
            {
                var tackleBreakYardsResult = new TackleBreakYardsSkillsCheckResult(_rng);
                tackleBreakYardsResult.Execute(game);
                adjustedYards += tackleBreakYardsResult.Result;
                play.Result.LogInformation($"{ballCarrier.LastName} breaks a tackle! Keeps churning!");
            }

            // Check for big run breakaway
            var bigRunCheck = new BigRunSkillsCheck(_rng, ballCarrier);
            bigRunCheck.Execute(game);

            if (bigRunCheck.Occurred)
            {
                var breakawayYardsResult = new BreakawayYardsSkillsCheckResult(_rng);
                breakawayYardsResult.Execute(game);
                adjustedYards += breakawayYardsResult.Result;
                play.Result.LogInformation($"{ballCarrier.LastName} breaks into the open field! He's got room to run!");
            }

            // Ensure we don't exceed field boundaries
            var yardsToGoal = 100 - game.FieldPosition;
            var maxLoss = -1 * game.FieldPosition; // Can't lose more yards than current field position (prevents going past own goal line)
            var finalYards = Math.Max(maxLoss, Math.Min(adjustedYards, yardsToGoal));

            // Create the run segment
            var segment = new RunSegment
            {
                BallCarrier = ballCarrier,
                YardsGained = finalYards,
                Direction = direction,
                EndedInFumble = false // Fumble check happens later in FumbleReturn state
            };

            play.RunSegments.Add(segment);
            play.YardsGained = finalYards;

            // Calculate current field position after the run
            var currentFieldPosition = game.FieldPosition + finalYards;

            // Check for tackle penalties on the ball carrier
            var tacklers = play.DefensePlayersOnField
                .Where(p => p.Position == Positions.LB || p.Position == Positions.OLB ||
                           p.Position == Positions.DE || p.Position == Positions.DT ||
                           p.Position == Positions.CB || p.Position == Positions.S || p.Position == Positions.FS)
                .OrderByDescending(p => p.Speed + p.Tackling)
                .Take(2)
                .ToList();

            var tacklePenaltyCheck = new TacklePenaltyOccurredSkillsCheck(
                _rng, ballCarrier, tacklers, TackleContext.BallCarrier);
            tacklePenaltyCheck.Execute(game);

            if (tacklePenaltyCheck.Occurred)
            {
                // TacklePenaltyOccurredSkillsCheck doesn't have PenaltyInstance yet - use old system
                CheckAndAddPenalty(game, play, tacklePenaltyCheck.PenaltyThatOccurred,
                    PenaltyOccuredWhen.During, play.OffensePlayersOnField, play.DefensePlayersOnField);
            }

            // **INJURY CHECK: Ball carrier after being tackled**
            var defendersInvolved = tacklers.Count;
            var isBigPlay = finalYards >= 20;
            var isOutOfBounds = currentFieldPosition <= 0 || currentFieldPosition >= 100;

            CheckForInjury(game, play, ballCarrier, defendersInvolved, isOutOfBounds, isBigPlay, false);

            // **INJURY CHECK: Tacklers (less frequent, but can happen)**
            foreach (var tackler in tacklers)
            {
                // Tacklers have lower injury rate (50% of ball carrier rate)
                if (_rng.NextDouble() < 0.5)
                {
                    CheckForInjury(game, play, tackler, 1, isOutOfBounds, isBigPlay, false);
                }
            }

            // Check for fumble (before logging the narrative)
            var fumbleCheck = new FumbleOccurredSkillsCheck(
                _rng,
                ballCarrier,
                play.DefensePlayersOnField,
                PlayType.Run,
                false);
            fumbleCheck.Execute(game);

            if (fumbleCheck.Occurred)
            {
                segment.EndedInFumble = true;
                HandleFumbleRecovery(game, play, ballCarrier, currentFieldPosition);
            }
            else
            {
                // Update elapsed time (run plays take 5-8 seconds)
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);

                // Log the play-by-play narrative
                LogRunPlayNarrative(play, ballCarrier, direction, blockingSuccess, finalYards, yardsToGoal);
            }
        }

        /// <summary>
        /// Checks if a player sustains an injury during the play and handles substitution if needed
        /// </summary>
        private void CheckForInjury(Game game, RunPlay play, Player player, int defendersInvolved, bool isOutOfBounds, bool isBigPlay, bool isSack)
        {
            // Skip if player is already injured
            if (player.IsInjured)
                return;

            var injuryCheck = new InjuryOccurredSkillsCheck(
                _rng,
                PlayType.Run,
                player,
                defendersInvolved,
                isOutOfBounds,
                isBigPlay,
                isSack);
            injuryCheck.Execute(game);

            if (injuryCheck.Occurred)
            {
                // Determine injury details
                var injuryEffect = new InjuryEffectSkillsCheckResult(_rng, player, PlayType.Run);
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
        private void LogInjury(RunPlay play, Injury injury)
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

            play.Result.LogInformation($"⚠️  {injury.InjuredPlayer.LastName} is down on the field! Trainers are attending to a {injuryTypeText} injury.");
        }

        /// <summary>
        /// Substitutes an injured player with a backup from the depth chart
        /// </summary>
        private void SubstituteInjuredPlayer(Game game, RunPlay play, Player injuredPlayer, Injury injury)
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

        private Player? DetermineBallCarrier(RunPlay play)
        {
            // Primary carrier is RB, but could be QB for scramble, or FB
            var rb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.RB);

            // Chance QB keeps it (scramble or option)
            if (_rng.NextDouble() < GameProbabilities.Rushing.QB_SCRAMBLE_PROBABILITY)
            {
                var qb = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
                if (qb != null)
                    return qb;
            }

            // Default to RB
            return rb ?? play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.QB);
        }

        private RunDirection DetermineRunDirection()
        {
            var directions = new[]
            {
                RunDirection.Left,
                RunDirection.Right,
                RunDirection.Middle,
                RunDirection.MiddleLeft,
                RunDirection.MiddleRight,
                RunDirection.UpTheMiddle,
                RunDirection.OffLeftTackle,
                RunDirection.OffRightTackle,
                RunDirection.Sweep
            };

            return directions[_rng.Next(directions.Length)];
        }

        private void LogRunPlayNarrative(RunPlay play, Player ballCarrier, RunDirection direction, bool blockingSuccess, int yards, int yardsToGoal)
        {
            var positionName = ballCarrier.Position == Positions.QB ? "quarterback" : "running back";
            var directionText = GetDirectionText(direction);

            if (blockingSuccess)
            {
                play.Result.LogInformation($"Great blocking up front! {ballCarrier.LastName} takes the handoff {directionText}");
            }
            else
            {
                play.Result.LogInformation($"Defenders penetrate the line! {ballCarrier.LastName} struggles {directionText}");
            }

            if (yards <= -2)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} is tackled in the backfield for a loss of {Math.Abs(yards)} yards!");
            }
            else if (yards <= 0)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} is stopped at the line of scrimmage!");
            }
            else if (yards <= 3)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} picks up {yards} yards before being brought down.");
            }
            else if (yards <= 8)
            {
                play.Result.LogInformation($"{ballCarrier.LastName} finds a seam and gains {yards} yards!");
            }
            else if (yards <= 15)
            {
                play.Result.LogInformation($"Nice run by {ballCarrier.LastName}! Picks up {yards} yards!");
            }
            else if (yards < yardsToGoal)
            {
                play.Result.LogInformation($"BIG RUN! {ballCarrier.LastName} races for {yards} yards before being tackled!");
            }
            else
            {
                play.Result.LogInformation($"TOUCHDOWN!!! {ballCarrier.LastName} takes it {yards} yards to the house!");
            }
        }

        private string GetDirectionText(RunDirection direction)
        {
            return direction switch
            {
                RunDirection.Left => "to the left side",
                RunDirection.Right => "to the right side",
                RunDirection.Middle => "up the middle",
                RunDirection.MiddleLeft => "up the middle-left gap",
                RunDirection.MiddleRight => "up the middle-right gap",
                RunDirection.UpTheMiddle => "straight ahead",
                RunDirection.OffLeftTackle => "off left tackle",
                RunDirection.OffRightTackle => "off right tackle",
                RunDirection.Sweep => "on the sweep",
                _ => "forward"
            };
        }

        private void HandleFumbleRecovery(Game game, RunPlay play, Player fumbler, int fumbleSpot)
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
                play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
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
                    play.ElapsedTime += 5.0 + (_rng.NextDouble() * 4.0);
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

                    play.ElapsedTime += 5.0 + (_rng.NextDouble() * 3.0);
                }
            }

            play.Fumbles.Add(fumble);
        }

        private void CheckAndAddPenalty(
            Game game,
            RunPlay play,
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
        /// NEW: Checks and adds a penalty using the IPenalty architecture.
        /// Properly determines which side committed the penalty based on penalty semantics.
        /// </summary>
        private void CheckAndAddPenaltyInstance(
            Game game,
            RunPlay play,
            IPenalty penaltyInstance,
            PenaltyOccuredWhen occurredWhen,
            List<Player> offensePlayersOnField,
            List<Player> defensePlayersOnField)
        {
            if (penaltyInstance == null)
            {
                return;
            }

            // IMPORTANT: Consume dummy NextDouble() for test compatibility
            // Old system (PenaltyEffectSkillsCheckResult) consumed NextDouble() for team selection
            // We don't need it (we know from penalty.CommittedBy), but tests expect it
            // Note: We don't consume Next() here because penalty.SelectPlayerWhoCommitted() does that
            _rng.NextDouble(); // Dummy: old system's team selection

            // Determine which side committed the penalty based on penalty's CommittedBy property
            TeamSide committedBy = penaltyInstance.CommittedBy;
            Possession calledOn;
            List<Player> eligiblePlayers;

            if (committedBy == TeamSide.Offense)
            {
                calledOn = play.Possession;
                eligiblePlayers = offensePlayersOnField;
            }
            else if (committedBy == TeamSide.Defense)
            {
                calledOn = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                eligiblePlayers = defensePlayersOnField;
            }
            else // TeamSide.Either
            {
                // For penalties that can be committed by either side, randomly select
                // (though this should be rare - most penalties are side-specific)
                var isOffense = _rng.NextDouble() < 0.5;
                calledOn = isOffense ? play.Possession : (play.Possession == Possession.Home ? Possession.Away : Possession.Home);
                eligiblePlayers = isOffense ? offensePlayersOnField : defensePlayersOnField;
                committedBy = isOffense ? TeamSide.Offense : TeamSide.Defense;
            }

            // Filter eligible players by position if penalty specifies eligible positions
            if (penaltyInstance.EligiblePositions != null && penaltyInstance.EligiblePositions.Count > 0)
            {
                eligiblePlayers = eligiblePlayers
                    .Where(p => penaltyInstance.CanBeCommittedBy(p, committedBy))
                    .ToList();
            }

            // If no eligible players, penalty can't occur
            if (eligiblePlayers.Count == 0)
            {
                return;
            }

            // Select player who committed the penalty using penalty's logic
            var playerWhoCommitted = penaltyInstance.SelectPlayerWhoCommitted(eligiblePlayers, _rng);

            if (playerWhoCommitted == null)
            {
                return;
            }

            // Calculate penalty yardage using penalty's logic
            var enforcementContext = new PenaltyEnforcementContext
            {
                FieldPosition = game.FieldPosition,
                InEndZone = false, // TODO: Determine based on play context
                SpotOfFoul = game.FieldPosition, // TODO: Calculate actual spot for spot fouls
                CommittedBy = committedBy
            };

            var yards = penaltyInstance.CalculateYardage(enforcementContext);

            // Determine if penalty should be accepted using penalty's logic
            // Convert Possession to TeamSide for context
            var committedByTeamSide = committedBy;
            var offenseTeamSide = TeamSide.Offense;

            var acceptanceContext = new PenaltyAcceptanceContext
            {
                CommittedBy = committedByTeamSide,
                Offense = offenseTeamSide,
                IsAutomaticFirstDown = penaltyInstance.IsAutomaticFirstDown,
                IsLossOfDown = penaltyInstance.IsLossOfDown,
                PenaltyYards = yards,
                YardsGainedOnPlay = play.YardsGained,
                CurrentDown = game.CurrentDown,
                YardsToGo = game.YardsToGo
            };

            var accepted = penaltyInstance.ShouldAccept(acceptanceContext);

            // Create penalty domain object
            // Map string name to PenaltyNames enum for backward compatibility
            var penaltyNameEnum = MapPenaltyNameToEnum(penaltyInstance.Name);

            var penalty = new Penalty
            {
                Name = penaltyNameEnum,
                CalledOn = calledOn,
                Player = playerWhoCommitted,
                OccuredWhen = occurredWhen,
                Yards = yards,
                Accepted = accepted
            };

            play.Penalties.Add(penalty);
            play.Result.LogInformation($"PENALTY: {penalty.Name} on {penalty.CalledOn}, {penalty.Yards} yards");
        }

        /// <summary>
        /// Maps IPenalty string names to PenaltyNames enum for backward compatibility.
        /// TODO: Remove this mapping once Penalty domain object is refactored to use IPenalty directly.
        /// </summary>
        private PenaltyNames MapPenaltyNameToEnum(string penaltyName)
        {
            return penaltyName switch
            {
                "Offensive Holding" => PenaltyNames.OffensiveHolding,
                "Defensive Holding" => PenaltyNames.DefensiveHolding,
                "False Start" => PenaltyNames.FalseStart,
                "Defensive Pass Interference" => PenaltyNames.DefensivePassInterference,
                "Delay of Game" => PenaltyNames.DelayofGame,
                "Defensive Offside" => PenaltyNames.DefensiveOffside,
                "Neutral Zone Infraction" => PenaltyNames.NeutralZoneInfraction,
                "Illegal Formation" => PenaltyNames.IllegalFormation,
                "Encroachment" => PenaltyNames.Encroachment,
                "Illegal Shift" => PenaltyNames.IllegalShift,
                "Illegal Motion" => PenaltyNames.IllegalMotion,
                "12 Men on Field (Offense)" => PenaltyNames.Offensive12OnField,
                "12 Men on Field (Defense)" => PenaltyNames.Defensive12OnField,
                "Illegal Substitution" => PenaltyNames.IllegalSubstitution,
                "Offensive Offside" => PenaltyNames.OffensiveOffside,
                "Illegal Contact" => PenaltyNames.IllegalContact,
                "Offensive Pass Interference" => PenaltyNames.OffensivePassInterference,
                "Unnecessary Roughness" => PenaltyNames.UnnecessaryRoughness,
                "Roughing the Passer" => PenaltyNames.RoughingthePasser,
                "Roughing the Kicker" => PenaltyNames.RoughingtheKicker,
                "Unsportsmanlike Conduct" => PenaltyNames.UnsportsmanlikeConduct,
                "Face Mask (15 Yards)" => PenaltyNames.FaceMask15Yards,
                "Horse Collar Tackle" => PenaltyNames.HorseCollarTackle,
                "Intentional Grounding" => PenaltyNames.IntentionalGrounding,
                "Illegal Forward Pass" => PenaltyNames.IllegalForwardPass,
                "Clipping" => PenaltyNames.Clipping,
                "Illegal Block Above the Waist" => PenaltyNames.IllegalBlockAbovetheWaist,
                "Illegal Use of Hands" => PenaltyNames.IllegalUseofHands,
                "Personal Foul" => PenaltyNames.PersonalFoul,
                "Taunting" => PenaltyNames.Taunting,
                _ => PenaltyNames.NoPenalty // Fallback for unmapped penalties
            };
        }
    }
}
