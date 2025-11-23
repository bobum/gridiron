using DomainObjects;
using DomainObjects.Helpers;
using System.Collections.Generic;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Represents an NFL penalty with all its rules and enforcement logic.
    /// Each specific penalty type implements this interface to encapsulate its unique rules.
    /// </summary>
    public interface IPenalty
    {
        // ==================== Identity ====================

        /// <summary>
        /// The official NFL name of this penalty (e.g., "Holding", "Pass Interference")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of what constitutes this penalty
        /// </summary>
        string Description { get; }

        // ==================== Occurrence ====================

        /// <summary>
        /// Base probability this penalty occurs per play (0.0 to 1.0)
        /// Example: 0.019 = 1.9% chance per play
        /// </summary>
        double BaseOccurrenceProbability { get; }

        /// <summary>
        /// Calculates the actual occurrence probability given the current game context.
        /// Adjusts base probability based on player skills, play type, situation, etc.
        /// </summary>
        /// <param name="context">Current game situation and players involved</param>
        /// <returns>Adjusted probability (0.0 to 1.0)</returns>
        double CalculateOccurrenceProbability(PenaltyContext context);

        /// <summary>
        /// When during a play this penalty can occur
        /// </summary>
        PenaltyTiming Timing { get; }

        // ==================== Eligibility ====================

        /// <summary>
        /// Which side of the ball commits this penalty
        /// </summary>
        TeamSide CommittedBy { get; }

        /// <summary>
        /// List of positions eligible to commit this penalty.
        /// Empty list means any position can commit it.
        /// </summary>
        List<Positions> EligiblePositions { get; }

        /// <summary>
        /// Determines if a specific player can commit this penalty based on their position and role
        /// </summary>
        /// <param name="player">Player to check</param>
        /// <param name="side">Whether player is on offense or defense for this play</param>
        /// <returns>True if player can commit this penalty</returns>
        bool CanBeCommittedBy(Player player, TeamSide side);

        // ==================== Enforcement ====================

        /// <summary>
        /// Calculates the yardage penalty for enforcement.
        /// Most penalties return a fixed value, but some (like DPI) vary by situation.
        /// </summary>
        /// <param name="context">Enforcement context including field position, play result, etc.</param>
        /// <returns>Yards to be penalized (positive number)</returns>
        int CalculateYardage(PenaltyEnforcementContext context);

        /// <summary>
        /// Whether this penalty gives an automatic first down (defensive penalties only)
        /// </summary>
        bool IsAutomaticFirstDown { get; }

        /// <summary>
        /// Whether this penalty causes loss of down (offensive penalties only)
        /// </summary>
        bool IsLossOfDown { get; }

        /// <summary>
        /// Whether this is a spot foul (enforced from where foul occurred)
        /// If false, enforced from previous line of scrimmage
        /// </summary>
        bool IsSpotFoul { get; }

        /// <summary>
        /// Whether this is a dead ball foul (occurs before snap, prevents play from executing)
        /// </summary>
        bool IsDeadBallFoul { get; }

        // ==================== Player Selection ====================

        /// <summary>
        /// Selects which player from the eligible list committed the penalty.
        /// Typically weighted by discipline (lower discipline = more likely).
        /// </summary>
        /// <param name="eligiblePlayers">Players who could have committed this penalty</param>
        /// <param name="rng">Random number generator for selection</param>
        /// <returns>The player who committed the penalty</returns>
        Player SelectPlayerWhoCommitted(List<Player> eligiblePlayers, ISeedableRandom rng);

        // ==================== Acceptance ====================

        /// <summary>
        /// Determines whether the penalty should be accepted or declined based on game situation.
        /// Most penalties are accepted, but sometimes declining is advantageous.
        /// </summary>
        /// <param name="context">Context for acceptance decision</param>
        /// <returns>True if penalty should be accepted</returns>
        bool ShouldAccept(PenaltyAcceptanceContext context);
    }

    /// <summary>
    /// Which side of the ball commits a penalty
    /// </summary>
    public enum TeamSide
    {
        /// <summary>Penalty committed by offensive team</summary>
        Offense,

        /// <summary>Penalty committed by defensive team</summary>
        Defense,

        /// <summary>Penalty can be committed by either side (e.g., Unnecessary Roughness)</summary>
        Either
    }

    /// <summary>
    /// When during a play a penalty can occur
    /// </summary>
    public enum PenaltyTiming
    {
        /// <summary>Before the snap (dead ball foul)</summary>
        PreSnap,

        /// <summary>During blocking/protection</summary>
        DuringBlocking,

        /// <summary>During pass coverage</summary>
        DuringCoverage,

        /// <summary>During the run/catch</summary>
        DuringAction,

        /// <summary>After the play is over</summary>
        PostPlay,

        /// <summary>Can occur at any time</summary>
        Any
    }

    /// <summary>
    /// Context information for determining if a penalty occurs
    /// </summary>
    public class PenaltyContext
    {
        public PlayType PlayType { get; set; }
        public List<Player> OffensivePlayers { get; set; } = new();
        public List<Player> DefensivePlayers { get; set; } = new();
        public int FieldPosition { get; set; }
        public Downs Down { get; set; }
        public int YardsToGo { get; set; }
        public int AirYards { get; set; }  // For pass plays
        public bool PassCompleted { get; set; }  // For coverage penalties
    }

    /// <summary>
    /// Context information for penalty enforcement
    /// </summary>
    public class PenaltyEnforcementContext
    {
        public int FieldPosition { get; set; }
        public TeamSide CommittedBy { get; set; }
        public int YardsGainedOnPlay { get; set; }
        public int SpotOfFoul { get; set; }  // For spot fouls
        public bool InEndZone { get; set; }  // For special end zone rules
    }

    /// <summary>
    /// Context information for penalty acceptance decisions
    /// </summary>
    public class PenaltyAcceptanceContext
    {
        public Downs CurrentDown { get; set; }
        public int YardsToGo { get; set; }
        public int YardsGainedOnPlay { get; set; }
        public int PenaltyYards { get; set; }
        public TeamSide CommittedBy { get; set; }
        public TeamSide Offense { get; set; }
        public bool IsAutomaticFirstDown { get; set; }
        public bool IsLossOfDown { get; set; }
    }
}
