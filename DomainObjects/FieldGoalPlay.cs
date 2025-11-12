using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects
{
    /// <summary>
    /// Represents a field goal or extra point attempt
    /// </summary>
    public class FieldGoalPlay : IPlay
    {
        // ========================================
        // IPLAY INTERFACE IMPLEMENTATION
        // ========================================

        public PlayType PlayType => PlayType.FieldGoal;
        public int StartTime { get; set; }
        public int StopTime { get; set; }
        public double ElapsedTime { get; set; }
        public Possession Possession { get; set; } = Possession.None;
        public Downs Down { get; set; }
        public ILogger Result { get; set; } = NullLogger.Instance;
        public List<Player> OffensePlayersOnField { get; set; } = new List<Player>();
        public List<Player> DefensePlayersOnField { get; set; } = new List<Player>();
        public List<Penalty> Penalties { get; set; } = new List<Penalty>();
        public List<Fumble> Fumbles { get; set; } = new List<Fumble>();
        public bool PossessionChange { get; set; }
        public bool Interception { get; set; }
        public int StartFieldPosition { get; set; }
        public int EndFieldPosition { get; set; }
        public int YardsGained { get; set; }
        public bool GoodSnap { get; set; }
        public bool QuarterExpired { get; set; }
        public bool HalfExpired { get; set; }
        public bool GameExpired { get; set; }
        public bool IsTouchdown { get; set; }
        public bool IsSafety { get; set; }

        // ========================================
        // FIELD GOAL-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// The kicker
        /// </summary>
        public Player? Kicker { get; set; }

        /// <summary>
        /// The holder
        /// </summary>
        public Player? Holder { get; set; }

        /// <summary>
        /// Attempt distance (from line of scrimmage + 17 yards for end zone depth and snap distance)
        /// </summary>
        public int AttemptDistance { get; set; }

        /// <summary>
        /// Whether the kick was good (made)
        /// </summary>
        public bool IsGood { get; set; }

        /// <summary>
        /// Whether the kick was blocked
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Whether this is an extra point (PAT) vs field goal
        /// </summary>
        public bool IsExtraPoint { get; set; }

        /// <summary>
        /// Player who blocked the kick
        /// </summary>
        public Player? BlockedBy { get; set; }

        /// <summary>
        /// Player who recovered the blocked kick
        /// </summary>
        public Player? RecoveredBy { get; set; }

        /// <summary>
        /// Yards gained/lost on recovery
        /// </summary>
        public int RecoveryYards { get; set; }

        /// <summary>
        /// If blocked and returned, the return segments
        /// </summary>
        public List<ReturnSegment>? BlockReturnSegments { get; set; }

        // ========================================
        // CONVENIENCE PROPERTIES
        // ========================================

        /// <summary>
        /// Total return yards on a blocked kick
        /// </summary>
        public int BlockReturnYards => BlockReturnSegments?.Sum(s => s.YardsGained) ?? 0;
    }
}
