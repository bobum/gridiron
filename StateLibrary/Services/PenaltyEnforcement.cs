using DomainObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.Services
{
    /// <summary>
    /// Centralized penalty enforcement service that handles all NFL penalty rules.
    /// Responsible for:
    /// - Penalty acceptance/decline logic
    /// - Multi-penalty resolution (offsetting, priority)
    /// - Enforcement (yards, automatic first downs, loss of down)
    /// - Half-distance to goal calculations
    /// - Dead ball vs live ball fouls
    /// </summary>
    public class PenaltyEnforcement
    {
        private readonly ILogger _logger;

        public PenaltyEnforcement(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Enforces penalties on a play and returns the enforcement result.
        /// Handles acceptance/decline, offsetting, and yardage calculation.
        /// </summary>
        public PenaltyEnforcementResult EnforcePenalties(
            Game game,
            IPlay play,
            int yardsGainedOnPlay)
        {
            var result = new PenaltyEnforcementResult
            {
                NetYards = yardsGainedOnPlay,
                NewDown = game.CurrentDown,
                NewYardsToGo = game.YardsToGo - yardsGainedOnPlay
            };

            if (play.Penalties == null || !play.Penalties.Any())
            {
                return result;
            }

            // Separate accepted penalties by team
            var acceptedPenalties = play.Penalties.Where(p => p.Accepted).ToList();

            if (!acceptedPenalties.Any())
            {
                return result;
            }

            var offensePenalties = acceptedPenalties.Where(p => p.CalledOn == play.Possession).ToList();
            var defensePenalties = acceptedPenalties.Where(p => p.CalledOn != play.Possession).ToList();

            // Check for offsetting penalties (both teams committed fouls)
            if (offensePenalties.Any() && defensePenalties.Any())
            {
                return HandleOffsettingPenalties(game, play, offensePenalties, defensePenalties, result);
            }

            // Single team committed penalty(ies)
            var penaltiesToEnforce = offensePenalties.Any() ? offensePenalties : defensePenalties;
            var penalizedTeam = offensePenalties.Any() ? play.Possession :
                (play.Possession == Possession.Home ? Possession.Away : Possession.Home);

            // For multiple penalties by same team, typically enforce the most severe
            var primaryPenalty = SelectPrimaryPenalty(penaltiesToEnforce);

            return EnforceSinglePenalty(game, play, primaryPenalty, penalizedTeam, yardsGainedOnPlay);
        }

        /// <summary>
        /// Handles offsetting penalties per NFL rules.
        /// Generally offset and replay down, except for new 2024 rule about major fouls.
        /// </summary>
        private PenaltyEnforcementResult HandleOffsettingPenalties(
            Game game,
            IPlay play,
            List<Penalty> offensePenalties,
            List<Penalty> defensePenalties,
            PenaltyEnforcementResult baseResult)
        {
            // 2024 Rule: Major (15-yard) offensive foul before change of possession
            // is not offset by minor (5-yard) defensive foul
            var offenseMajor = offensePenalties.Any(p => p.Yards >= 15);
            var defenseOnlyMinor = defensePenalties.All(p => p.Yards <= 5);

            if (offenseMajor && defenseOnlyMinor && !play.PossessionChange)
            {
                // Enforce the major offensive penalty
                var majorPenalty = offensePenalties.First(p => p.Yards >= 15);
                _logger?.LogInformation($"Offsetting penalties - Major offensive foul enforced: {majorPenalty.Name}");
                return EnforceSinglePenalty(game, play, majorPenalty, play.Possession, 0);
            }

            // Standard offsetting - replay the down
            _logger?.LogInformation("Offsetting penalties - Down will be replayed");

            return new PenaltyEnforcementResult
            {
                NetYards = 0,
                NewDown = play.Down, // Replay same down
                NewYardsToGo = game.YardsToGo,
                IsOffsetting = true,
                OffensingPenalties = offensePenalties,
                DefensePenalties = defensePenalties
            };
        }

        /// <summary>
        /// Selects the primary penalty when multiple penalties by same team.
        /// Typically the most severe (highest yardage) is enforced.
        /// </summary>
        private Penalty SelectPrimaryPenalty(List<Penalty> penalties)
        {
            // Prioritize by severity (yardage), then by occurrence order
            return penalties.OrderByDescending(p => p.Yards).ThenBy(p => p.OccuredWhen).First();
        }

        /// <summary>
        /// Enforces a single penalty and calculates the result.
        /// </summary>
        private PenaltyEnforcementResult EnforceSinglePenalty(
            Game game,
            IPlay play,
            Penalty penalty,
            Possession penalizedTeam,
            int yardsGainedOnPlay)
        {
            var isOffensivePenalty = penalizedTeam == play.Possession;
            var isDefensivePenalty = !isOffensivePenalty;

            // Calculate penalty yardage with half-distance rule
            var penaltyYards = CalculatePenaltyYards(game.FieldPosition, penalty.Yards, isOffensivePenalty);

            // Determine if penalty gives automatic first down
            var automaticFirstDown = isDefensivePenalty && IsAutomaticFirstDown(penalty.Name);

            // Determine if penalty causes loss of down
            var lossOfDown = isOffensivePenalty && IsLossOfDown(penalty.Name);

            // Calculate net yards (play result + penalty)
            int netYards;
            Downs newDown;
            int newYardsToGo;

            if (isOffensivePenalty)
            {
                // Offensive penalty - yards marked off from previous spot (usually)
                // Exception: Some penalties enforced from spot of foul
                netYards = IsSpotFoul(penalty.Name) ? yardsGainedOnPlay - penaltyYards : -penaltyYards;

                if (lossOfDown)
                {
                    newDown = AdvanceDown(play.Down);
                    newYardsToGo = game.YardsToGo + penaltyYards;
                }
                else
                {
                    // Same down, add penalty yards to distance needed
                    newDown = play.Down;
                    newYardsToGo = game.YardsToGo + penaltyYards;
                }
            }
            else
            {
                // Defensive penalty - yards marked off in favor of offense
                // Most spot fouls are defensive (DPI, etc.)
                if (IsSpotFoul(penalty.Name))
                {
                    // Spot foul - enforce from where foul occurred (already in yardsGainedOnPlay for DPI)
                    netYards = yardsGainedOnPlay + penaltyYards;
                }
                else
                {
                    // From previous spot - add penalty yards to play result
                    netYards = yardsGainedOnPlay + penaltyYards;
                }

                if (automaticFirstDown)
                {
                    newDown = Downs.First;
                    newYardsToGo = 10;
                }
                else
                {
                    // Check if penalty yardage gained first down
                    if (netYards >= game.YardsToGo)
                    {
                        newDown = Downs.First;
                        newYardsToGo = 10;
                    }
                    else
                    {
                        newDown = play.Down;
                        newYardsToGo = game.YardsToGo - netYards;
                    }
                }
            }

            _logger?.LogInformation($"PENALTY: {penalty.Name} on {(isOffensivePenalty ? "offense" : "defense")}. " +
                $"{Math.Abs(penaltyYards)} yards {(automaticFirstDown ? "and automatic first down" : "")}");

            return new PenaltyEnforcementResult
            {
                NetYards = netYards,
                NewDown = newDown,
                NewYardsToGo = newYardsToGo,
                PenaltyEnforced = penalty,
                AutomaticFirstDown = automaticFirstDown,
                LossOfDown = lossOfDown
            };
        }

        /// <summary>
        /// Calculates actual penalty yards with half-distance to goal rule.
        /// </summary>
        private int CalculatePenaltyYards(int currentFieldPosition, int penaltyYards, bool isOffensivePenalty)
        {
            if (isOffensivePenalty)
            {
                // Offensive penalty - moving backward toward own goal
                var yardsToOwnGoal = currentFieldPosition;
                var halfDistance = yardsToOwnGoal / 2;

                // If penalty would move past half the distance to goal, use half distance
                return penaltyYards > halfDistance ? halfDistance : penaltyYards;
            }
            else
            {
                // Defensive penalty - moving forward toward opponent's goal
                var yardsToOpponentGoal = 100 - currentFieldPosition;
                var halfDistance = yardsToOpponentGoal / 2;

                // If penalty would move past half the distance to goal, use half distance
                return penaltyYards > halfDistance ? halfDistance : penaltyYards;
            }
        }

        /// <summary>
        /// Determines if a penalty gives an automatic first down.
        /// Per NFL rules, most defensive penalties give automatic first down,
        /// but there are exceptions (offside, encroachment, delay of game, etc.)
        /// </summary>
        private bool IsAutomaticFirstDown(PenaltyNames penalty)
        {
            // Exceptions - defensive penalties that do NOT give automatic first down
            var noAutomaticFirstDown = new[]
            {
                PenaltyNames.DefensiveOffside,
                PenaltyNames.Encroachment,
                PenaltyNames.NeutralZoneInfraction,
                PenaltyNames.DefensiveDelayofGame,
                PenaltyNames.IllegalSubstitution,
                PenaltyNames.Defensive12OnField,
                PenaltyNames.RunningIntotheKicker
            };

            return !noAutomaticFirstDown.Contains(penalty);
        }

        /// <summary>
        /// Determines if a penalty causes loss of down.
        /// Only specific offensive penalties cause loss of down.
        /// </summary>
        private bool IsLossOfDown(PenaltyNames penalty)
        {
            var lossOfDownPenalties = new[]
            {
                PenaltyNames.IntentionalGrounding,
                PenaltyNames.IllegalForwardPass
            };

            return lossOfDownPenalties.Contains(penalty);
        }

        /// <summary>
        /// Determines if penalty is a spot foul (enforced from spot of foul).
        /// Most notably Defensive Pass Interference.
        /// </summary>
        private bool IsSpotFoul(PenaltyNames penalty)
        {
            var spotFouls = new[]
            {
                PenaltyNames.DefensivePassInterference
            };

            return spotFouls.Contains(penalty);
        }

        /// <summary>
        /// Determines if a penalty is a dead ball foul (occurs before snap).
        /// Dead ball fouls prevent the play from executing.
        /// </summary>
        public bool IsDeadBallFoul(PenaltyNames penalty)
        {
            var deadBallFouls = new[]
            {
                PenaltyNames.FalseStart,
                PenaltyNames.Encroachment,
                PenaltyNames.DelayofGame,
                PenaltyNames.DefensiveDelayofGame,
                PenaltyNames.Offensive12OnField,
                PenaltyNames.Defensive12OnField,
                PenaltyNames.IllegalSubstitution
            };

            return deadBallFouls.Contains(penalty);
        }

        /// <summary>
        /// Determines whether to accept or decline a penalty based on game situation.
        /// </summary>
        public bool ShouldAcceptPenalty(
            Game game,
            Penalty penalty,
            Possession penalizedTeam,
            Possession offense,
            int yardsGainedOnPlay,
            Downs currentDown,
            int yardsToGo)
        {
            var isOffensivePenalty = penalizedTeam == offense;
            var isDefensivePenalty = !isOffensivePenalty;

            // Calculate what would happen if accepted vs declined
            var acceptedYards = isOffensivePenalty ? -penalty.Yards : penalty.Yards;
            var acceptedDown = currentDown;
            var acceptedYardsToGo = yardsToGo;

            if (isDefensivePenalty && IsAutomaticFirstDown(penalty.Name))
            {
                acceptedDown = Downs.First;
                acceptedYardsToGo = 10;
            }
            else if (isOffensivePenalty && IsLossOfDown(penalty.Name))
            {
                acceptedDown = AdvanceDown(currentDown);
            }

            // Declined outcome
            var declinedYards = yardsGainedOnPlay;
            var declinedDown = currentDown;
            var declinedYardsToGo = yardsToGo - yardsGainedOnPlay;

            if (declinedYards >= yardsToGo)
            {
                declinedDown = Downs.First;
                declinedYardsToGo = 10;
            }
            else if (declinedYards < 0)
            {
                declinedDown = AdvanceDown(currentDown);
                declinedYardsToGo = yardsToGo - declinedYards;
            }

            // Decision logic varies by which team benefits
            if (isDefensivePenalty)
            {
                // Defensive penalty - offense decides
                // Accept if it gives better field position or down/distance

                // Always accept if it gives automatic first down
                if (IsAutomaticFirstDown(penalty.Name))
                {
                    return true;
                }

                // Accept if penalty yards are better than play result
                if (acceptedYards > declinedYards)
                {
                    return true;
                }

                // Decline if play result was better (e.g., long completion vs 5-yard penalty)
                return false;
            }
            else
            {
                // Offensive penalty - defense decides

                // Generally accept offensive penalties (they help defense)
                // Exception: Decline if play result was worse for offense

                // Example: 3rd and 10, offense gains 2 yards with holding penalty
                // Declined: 4th and 8 (better for defense - forces punt/4th down attempt)
                // Accepted: 3rd and 20 (gives offense another chance)

                if (declinedDown == Downs.Fourth || declinedDown == Downs.None)
                {
                    // Decline - play result already caused 4th down or turnover
                    return false;
                }

                // Accept in most other cases
                return true;
            }
        }

        private Downs AdvanceDown(Downs currentDown)
        {
            return currentDown switch
            {
                Downs.First => Downs.Second,
                Downs.Second => Downs.Third,
                Downs.Third => Downs.Fourth,
                Downs.Fourth => Downs.None,
                _ => Downs.None
            };
        }
    }

    /// <summary>
    /// Result of penalty enforcement on a play.
    /// </summary>
    public class PenaltyEnforcementResult
    {
        /// <summary>
        /// Net yards after applying penalty (could be positive or negative).
        /// </summary>
        public int NetYards { get; set; }

        /// <summary>
        /// New down after penalty enforcement.
        /// </summary>
        public Downs NewDown { get; set; }

        /// <summary>
        /// New yards to go after penalty enforcement.
        /// </summary>
        public int NewYardsToGo { get; set; }

        /// <summary>
        /// The primary penalty that was enforced (if any).
        /// </summary>
        public Penalty PenaltyEnforced { get; set; }

        /// <summary>
        /// Whether this penalty gave an automatic first down.
        /// </summary>
        public bool AutomaticFirstDown { get; set; }

        /// <summary>
        /// Whether this penalty caused loss of down.
        /// </summary>
        public bool LossOfDown { get; set; }

        /// <summary>
        /// Whether penalties offset (both teams committed fouls).
        /// </summary>
        public bool IsOffsetting { get; set; }

        /// <summary>
        /// Offensive penalties (for offsetting situations).
        /// </summary>
        public List<Penalty> OffensingPenalties { get; set; }

        /// <summary>
        /// Defensive penalties (for offsetting situations).
        /// </summary>
        public List<Penalty> DefensePenalties { get; set; }
    }
}
