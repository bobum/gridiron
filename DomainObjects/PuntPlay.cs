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
        public List<Injury> Injuries { get; set; } = new List<Injury>();
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
        public bool ClockStopped { get; set; }

        // ========================================
        // PUNT-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// The punter
        /// </summary>
        public Player? Punter { get; set; }

        /// <summary>
        /// The long snapper
        /// </summary>
        public Player? LongSnapper { get; set; }

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

        /// <summary>
        /// Whether the punt went out of bounds
        /// </summary>
        public bool OutOfBounds { get; set; }

        /// <summary>
        /// Player who blocked the punt
        /// </summary>
        public Player? BlockedBy { get; set; }

        /// <summary>
        /// Player who recovered the punt (blocked or muffed)
        /// </summary>
        public Player? RecoveredBy { get; set; }

        /// <summary>
        /// Yards gained after recovering blocked/muffed punt
        /// </summary>
        public int RecoveryYards { get; set; }

        /// <summary>
        /// Whether the returner muffed the catch
        /// </summary>
        public bool MuffedCatch { get; set; }

        /// <summary>
        /// Player who muffed the catch
        /// </summary>
        public Player? MuffedBy { get; set; }

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
