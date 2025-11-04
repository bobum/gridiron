using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects
{
    /// <summary>
    /// Represents a running play (handoff, QB scramble, QB kneel, etc.)
    /// </summary>
    public class RunPlay : IPlay
    {
        // ========================================
        // IPLAY INTERFACE IMPLEMENTATION
        // ========================================

        public PlayType PlayType => PlayType.Run;
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

        // ========================================
        // RUN-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// Segments of the run (handles multiple fumbles and ball carrier changes)
        /// </summary>
        public List<RunSegment> RunSegments { get; set; } = new List<RunSegment>();

        // ========================================
        // CONVENIENCE PROPERTIES
        // ========================================

        /// <summary>
        /// The initial ball carrier
        /// </summary>
        public Player? InitialBallCarrier => RunSegments.FirstOrDefault()?.BallCarrier;

        /// <summary>
        /// The final ball carrier (end of play)
        /// </summary>
        public Player? FinalBallCarrier => RunSegments.LastOrDefault()?.BallCarrier;

        /// <summary>
        /// Total yards gained across all segments
        /// </summary>
        public int TotalYards => RunSegments.Sum(s => s.YardsGained);

        /// <summary>
        /// Whether any fumbles occurred during the play
        /// </summary>
        public bool HadFumbles => RunSegments.Any(s => s.EndedInFumble);
    }
}
