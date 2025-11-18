using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Central registry for all penalty instances.
    /// Provides singleton instances of each penalty type and lookup methods.
    /// </summary>
    public static class PenaltyRegistry
    {
        // Singleton instances
        private static readonly OffensiveHoldingPenalty _offensiveHolding = new OffensiveHoldingPenalty();
        private static readonly DefensiveHoldingPenalty _defensiveHolding = new DefensiveHoldingPenalty();
        private static readonly FalseStartPenalty _falseStart = new FalseStartPenalty();
        private static readonly DefensivePassInterferencePenalty _defensivePassInterference = new DefensivePassInterferencePenalty();

        /// <summary>
        /// All registered penalties. Expand as more penalty classes are added.
        /// </summary>
        private static readonly List<IPenalty> _allPenalties = new List<IPenalty>
        {
            _offensiveHolding,
            _defensiveHolding,
            _falseStart,
            _defensivePassInterference
        };

        // ==================== Direct Accessors ====================

        public static OffensiveHoldingPenalty OffensiveHolding => _offensiveHolding;
        public static DefensiveHoldingPenalty DefensiveHolding => _defensiveHolding;
        public static FalseStartPenalty FalseStart => _falseStart;
        public static DefensivePassInterferencePenalty DefensivePassInterference => _defensivePassInterference;

        // ==================== Lookup Methods ====================

        /// <summary>
        /// Gets all registered penalties.
        /// </summary>
        public static IReadOnlyList<IPenalty> GetAll() => _allPenalties.AsReadOnly();

        /// <summary>
        /// Gets a penalty by name.
        /// </summary>
        public static IPenalty GetByName(string name)
        {
            return _allPenalties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all penalties that can occur at a specific timing.
        /// </summary>
        public static IEnumerable<IPenalty> GetByTiming(PenaltyTiming timing)
        {
            return _allPenalties.Where(p => p.Timing == timing || p.Timing == PenaltyTiming.Any);
        }

        /// <summary>
        /// Gets all penalties committed by a specific side.
        /// </summary>
        public static IEnumerable<IPenalty> GetBySide(TeamSide side)
        {
            return _allPenalties.Where(p => p.CommittedBy == side || p.CommittedBy == TeamSide.Either);
        }

        /// <summary>
        /// Gets all penalties that are dead ball fouls.
        /// </summary>
        public static IEnumerable<IPenalty> GetDeadBallFouls()
        {
            return _allPenalties.Where(p => p.IsDeadBallFoul);
        }

        /// <summary>
        /// Gets all penalties that give automatic first down.
        /// </summary>
        public static IEnumerable<IPenalty> GetAutomaticFirstDownPenalties()
        {
            return _allPenalties.Where(p => p.IsAutomaticFirstDown);
        }

        /// <summary>
        /// Gets penalties eligible for a specific context.
        /// Filters by timing, side, and player eligibility.
        /// </summary>
        public static IEnumerable<IPenalty> GetEligiblePenalties(PenaltyContext context, TeamSide side)
        {
            var players = side == TeamSide.Offense ? context.OffensivePlayers : context.DefensivePlayers;

            return _allPenalties.Where(p =>
            {
                // Must be committable by this side
                if (p.CommittedBy != side && p.CommittedBy != TeamSide.Either)
                    return false;

                // Must have eligible players
                if (players != null && players.Any())
                {
                    var hasEligiblePlayer = players.Any(player => p.CanBeCommittedBy(player, side));
                    if (!hasEligiblePlayer)
                        return false;
                }

                return true;
            });
        }
    }
}
