using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DomainObjects
{
    /// <summary>
    /// Interface for all play types in a football game
    /// </summary>
    public interface IPlay
    {
        // ========================================
        // IDENTITY & TYPE
        // ========================================

        /// <summary>
        /// The type of play (determined by concrete implementation)
        /// </summary>
        PlayType PlayType { get; }

        // ========================================
        // TIMING
        // ========================================

        /// <summary>
        /// Game time when play started (in seconds)
        /// </summary>
        int StartTime { get; set; }

        /// <summary>
        /// Game time when play ended (in seconds)
        /// </summary>
        int StopTime { get; set; }

        /// <summary>
        /// Time elapsed during this play (in seconds)
        /// </summary>
        double ElapsedTime { get; set; }

        // ========================================
        // GAME CONTEXT
        // ========================================

        /// <summary>
        /// Which team has possession
        /// </summary>
        Possession Possession { get; set; }

        /// <summary>
        /// Which down (1st, 2nd, 3rd, 4th, or None for kickoffs)
        /// </summary>
        Downs Down { get; set; }

        // ========================================
        // LOGGING
        // ========================================

        /// <summary>
        /// Logger for play-by-play output
        /// </summary>
        ILogger Result { get; set; }

        // ========================================
        // PLAYERS ON FIELD
        // ========================================

        /// <summary>
        /// Offensive players on field for this play
        /// </summary>
        List<Player> OffensePlayersOnField { get; set; }

        /// <summary>
        /// Defensive players on field for this play
        /// </summary>
        List<Player> DefensePlayersOnField { get; set; }

        // ========================================
        // EVENTS (can happen on ANY play)
        // ========================================

        /// <summary>
        /// Penalties that occurred during this play
        /// </summary>
        List<Penalty> Penalties { get; set; }

        /// <summary>
        /// Fumbles that occurred during this play (can be multiple)
        /// </summary>
        List<Fumble> Fumbles { get; set; }

        /// <summary>
        /// Injuries that occurred during this play
        /// </summary>
        List<Injury> Injuries { get; set; }

        /// <summary>
        /// Whether possession changed during this play
        /// </summary>
        bool PossessionChange { get; set; }

        /// <summary>
        /// Whether the play resulted in an interception (only applicable to pass plays)
        /// </summary>
        bool Interception { get; set; }

        // ========================================
        // FIELD POSITION & YARDAGE
        // ========================================

        /// <summary>
        /// Yard line where play started (0-100 from offense's perspective)
        /// </summary>
        int StartFieldPosition { get; set; }

        /// <summary>
        /// Yard line where play ended (0-100 from offense's perspective)
        /// </summary>
        int EndFieldPosition { get; set; }

        /// <summary>
        /// Net yards gained on the play (can be negative)
        /// </summary>
        int YardsGained { get; set; }

        // ========================================
        // EXECUTION STATE
        // ========================================

        /// <summary>
        /// Whether the snap was good (kickoffs ignore this)
        /// </summary>
        bool GoodSnap { get; set; }

        // ========================================
        // GAME CLOCK EXPIRATION
        // ========================================

        /// <summary>
        /// Whether this play ended the quarter
        /// </summary>
        bool QuarterExpired { get; set; }

        /// <summary>
        /// Whether this play ended the half
        /// </summary>
        bool HalfExpired { get; set; }

        /// <summary>
        /// Whether this play ended the game
        /// </summary>
        bool GameExpired { get; set; }

        // ========================================
        // SCORING
        // ========================================

        /// <summary>
        /// Whether this play resulted in a touchdown
        /// </summary>
        bool IsTouchdown { get; set; }

        /// <summary>
        /// Whether this play resulted in a safety
        /// </summary>
        bool IsSafety { get; set; }
    }
}
