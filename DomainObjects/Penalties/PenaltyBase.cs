using DomainObjects;
using DomainObjects.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Base class for all penalties providing common default implementations.
    /// Most penalties only need to override simple properties (Name, Yards, etc.)
    /// and can use the default behavior for player selection, probability calculation, etc.
    /// </summary>
    public abstract class PenaltyBase : IPenalty
    {
        // ==================== Identity (must override) ====================

        public abstract string Name { get; }

        public virtual string Description => $"{Name} penalty";

        // ==================== Occurrence (must override probability) ====================

        public abstract double BaseOccurrenceProbability { get; }

        public virtual PenaltyTiming Timing => PenaltyTiming.Any;

        /// <summary>
        /// Default implementation: returns base probability with no adjustments.
        /// Override this for penalties that vary based on context (e.g., more holding on pass plays).
        /// </summary>
        public virtual double CalculateOccurrenceProbability(PenaltyContext context)
        {
            return BaseOccurrenceProbability;
        }

        // ==================== Eligibility (must override CommittedBy) ====================

        public abstract TeamSide CommittedBy { get; }

        /// <summary>
        /// Default: empty list means any position can commit.
        /// Override for position-specific penalties (e.g., False Start only by offensive linemen).
        /// </summary>
        public virtual List<Positions> EligiblePositions => new List<Positions>();

        /// <summary>
        /// Default implementation: checks if player is on correct side and in eligible positions.
        /// </summary>
        public virtual bool CanBeCommittedBy(Player player, TeamSide side)
        {
            // Check if player is on the correct side
            if (CommittedBy != TeamSide.Either && CommittedBy != side)
            {
                return false;
            }

            // If no position restrictions, any player on correct side can commit
            if (EligiblePositions == null || EligiblePositions.Count == 0)
            {
                return true;
            }

            // Check if player's position is eligible
            return EligiblePositions.Contains(player.Position);
        }

        // ==================== Enforcement (must override Yards) ====================

        /// <summary>
        /// Most penalties have fixed yardage. Override CalculateYardage for variable penalties (DPI).
        /// </summary>
        public abstract int Yards { get; }

        /// <summary>
        /// Default implementation: returns fixed yardage.
        /// Override for spot fouls or context-dependent yardage.
        /// </summary>
        public virtual int CalculateYardage(PenaltyEnforcementContext context)
        {
            return Yards;
        }

        public virtual bool IsAutomaticFirstDown => false;

        public virtual bool IsLossOfDown => false;

        public virtual bool IsSpotFoul => false;

        public virtual bool IsDeadBallFoul => false;

        // ==================== Player Selection ====================

        /// <summary>
        /// Default implementation: selects player weighted by discipline.
        /// Players with lower discipline are more likely to commit penalties.
        /// Uses same weighting algorithm as existing PenaltyEffectSkillsCheckResult.
        /// </summary>
        public virtual Player SelectPlayerWhoCommitted(List<Player> eligiblePlayers, ISeedableRandom rng)
        {
            if (eligiblePlayers == null || eligiblePlayers.Count == 0)
            {
                return null;
            }

            // If only one player, return them
            if (eligiblePlayers.Count == 1)
            {
                return eligiblePlayers[0];
            }

            // Calculate penalty weights for each player (lower discipline = higher weight)
            // Weight = (100 - Discipline) + 20 (to ensure minimum weight)
            // This means discipline 0 = weight 120, discipline 100 = weight 20
            var weights = new List<int>();
            int totalWeight = 0;

            foreach (var player in eligiblePlayers)
            {
                // Use discipline value directly (0-100 range)
                // Discipline 0 = undisciplined (weight 120)
                // Discipline 100 = highly disciplined (weight 20)
                var discipline = player.Discipline;
                var weight = (100 - discipline) + 20; // Inverse relationship
                weights.Add(weight);
                totalWeight += weight;
            }

            // Select player using weighted random selection
            // Use Next() for compatibility with existing tests
            var roll = rng.Next(totalWeight);
            var cumulativeWeight = 0;

            for (int i = 0; i < eligiblePlayers.Count; i++)
            {
                cumulativeWeight += weights[i];
                if (roll < cumulativeWeight)
                {
                    return eligiblePlayers[i];
                }
            }

            // Fallback (should never reach here)
            return eligiblePlayers[eligiblePlayers.Count - 1];
        }

        // ==================== Acceptance ====================

        /// <summary>
        /// Default implementation: basic acceptance logic.
        /// Override for penalties with special acceptance considerations.
        /// </summary>
        public virtual bool ShouldAccept(PenaltyAcceptanceContext context)
        {
            var isOffensivePenalty = context.CommittedBy == context.Offense;
            var isDefensivePenalty = !isOffensivePenalty;

            if (isDefensivePenalty)
            {
                // Defensive penalty - offense decides

                // Always accept if it gives automatic first down
                if (context.IsAutomaticFirstDown)
                {
                    return true;
                }

                // Accept if penalty yards are better than play result
                if (context.PenaltyYards > context.YardsGainedOnPlay)
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

                var declinedDown = context.CurrentDown;
                var declinedYardsToGo = context.YardsToGo - context.YardsGainedOnPlay;

                if (context.YardsGainedOnPlay >= context.YardsToGo)
                {
                    // Play gained first down - always accept penalty to negate it
                    return true;
                }

                if (declinedDown == Downs.Fourth || declinedDown == Downs.None)
                {
                    // Decline - play result already caused 4th down or turnover
                    return false;
                }

                // Accept in most other cases
                return true;
            }
        }
    }
}
