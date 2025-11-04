using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects
{
    /// <summary>
    /// Represents a passing play (forward pass, laterals, etc.)
    /// </summary>
    public class PassPlay : IPlay
    {
        // ========================================
        // IPLAY INTERFACE IMPLEMENTATION
        // ========================================

        public PlayType PlayType => PlayType.Pass;
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
        // PASS-SPECIFIC PROPERTIES
        // ========================================

        /// <summary>
        /// Segments of the pass play (handles laterals and multiple passers)
        /// </summary>
        public List<PassSegment> PassSegments { get; set; } = new List<PassSegment>();

        /// <summary>
        /// Interception details (only applicable to pass plays)
        /// </summary>
        public Interception? InterceptionDetails { get; set; }

        // ========================================
        // CONVENIENCE PROPERTIES
        // ========================================

        /// <summary>
        /// The primary/initial passer (usually the QB)
        /// </summary>
        public Player? PrimaryPasser => PassSegments.FirstOrDefault()?.Passer;

        /// <summary>
        /// The final receiver (end of play)
        /// </summary>
        public Player? FinalReceiver => PassSegments.LastOrDefault()?.Receiver;

        /// <summary>
        /// Whether the initial pass was completed
        /// </summary>
        public bool IsComplete => PassSegments.FirstOrDefault()?.IsComplete ?? false;

        /// <summary>
        /// Total air yards across all pass segments
        /// </summary>
        public int TotalAirYards => PassSegments.Sum(s => s.AirYards);

        /// <summary>
        /// Total yards gained across all segments
        /// </summary>
        public int TotalYards => PassSegments.Sum(s => s.YardsGained);

        /// <summary>
        /// Whether the play included any lateral passes
        /// </summary>
        public bool HadLaterals => PassSegments.Count > 1;

        /// <summary>
        /// Whether any fumbles occurred during the play
        /// </summary>
        public bool HadFumbles => PassSegments.Any(s => s.EndedInFumble);
    }
}
