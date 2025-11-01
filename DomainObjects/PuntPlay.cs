using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects
{
    /// <summary>
    /// Represents a punt play
    /// </summary>
    public class PuntPlay : IPlay
    {
        // ========================================
        // IPLAY INTERFACE IMPLEMENTATION
        // ========================================

        public PlayType PlayType => PlayType.Punt;
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
        // PUNT-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// The punter
        /// </summary>
        public Player? Punter { get; set; }

        /// <summary>
        /// Distance the punt traveled in the air
        /// </summary>
        public int PuntDistance { get; set; }

        /// <summary>
        /// How long the ball was in the air (seconds)
        /// </summary>
        public double HangTime { get; set; }

        /// <summary>
        /// Return segments (handles multiple fumbles on return)
        /// </summary>
        public List<ReturnSegment> ReturnSegments { get; set; } = new List<ReturnSegment>();

        /// <summary>
        /// Whether a fair catch was signaled and made
        /// </summary>
        public bool FairCatch { get; set; }

        /// <summary>
        /// Whether the punt resulted in a touchback
        /// </summary>
        public bool Touchback { get; set; }

        /// <summary>
        /// Whether the punt was blocked
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Whether the punt was downed (not returned)
        /// </summary>
        public bool Downed { get; set; }

        /// <summary>
        /// Yard line where punt was downed (if Downed is true)
        /// </summary>
        public int DownedAtYardLine { get; set; }

        // ========================================
        // CONVENIENCE PROPERTIES
        // ========================================

        /// <summary>
        /// The initial returner
        /// </summary>
        public Player? InitialReturner => ReturnSegments.FirstOrDefault()?.BallCarrier;

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
