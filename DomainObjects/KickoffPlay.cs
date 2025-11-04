using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects
{
    /// <summary>
    /// Represents a kickoff play (start of half, after score)
    /// </summary>
    public class KickoffPlay : IPlay
    {
        // ========================================
        // IPLAY INTERFACE IMPLEMENTATION
        // ========================================

        public PlayType PlayType => PlayType.Kickoff;
        public int StartTime { get; set; }
        public int StopTime { get; set; }
        public double ElapsedTime { get; set; }
        public Possession Possession { get; set; } = Possession.None;
        public Downs Down { get; set; }  // Always Downs.None for kickoffs
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
        public bool GoodSnap { get; set; }  // Not applicable to kickoffs
        public bool QuarterExpired { get; set; }
        public bool HalfExpired { get; set; }
        public bool GameExpired { get; set; }
        public bool IsTouchdown { get; set; }

        // ========================================
        // KICKOFF-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// The kicker
        /// </summary>
        public Player? Kicker { get; set; }

        /// <summary>
        /// Distance the kickoff traveled
        /// </summary>
        public int KickDistance { get; set; }

        /// <summary>
        /// Return segments (handles multiple fumbles on return)
        /// </summary>
        public List<ReturnSegment> ReturnSegments { get; set; } = new List<ReturnSegment>();

        /// <summary>
        /// Whether the kickoff resulted in a touchback
        /// </summary>
        public bool Touchback { get; set; }

        /// <summary>
        /// Whether the kickoff went out of bounds
        /// </summary>
        public bool OutOfBounds { get; set; }

        /// <summary>
        /// Whether this was an onside kick attempt
        /// </summary>
        public bool OnsideKick { get; set; }

        /// <summary>
        /// Whether the onside kick was recovered
        /// </summary>
        public bool OnsideRecovered { get; set; }

        /// <summary>
        /// Which team recovered the onside kick
        /// </summary>
        public Team? OnsideRecoveredBy { get; set; }

        /// <summary>
        /// Player who recovered the onside kick or fumble
        /// </summary>
        public Player? RecoveredBy { get; set; }

        // ========================================
        // CONVENIENCE PROPERTIES
        // ========================================

        /// <summary>
        /// The initial returner
        /// </summary>
        public Player? InitialReturner => ReturnSegments.FirstOrDefault()?.BallCarrier;

        /// <summary>
        /// The final returner (end of play)
        /// </summary>
        public Player? FinalReturner => ReturnSegments.LastOrDefault()?.BallCarrier;

        /// <summary>
        /// Total return yards across all segments
        /// </summary>
        public int TotalReturnYards => ReturnSegments.Sum(s => s.YardsGained);

        /// <summary>
        /// Whether any fumbles occurred on the return
        /// </summary>
        public bool HadFumbles => ReturnSegments.Any(s => s.EndedInFumble);
    }
}
