namespace DomainObjects
{
    /// <summary>
    /// Represents a segment of a play where the ball carrier may change
    /// (e.g., fumble recovery, lateral, etc.)
    /// </summary>
    public interface IPlaySegment
    {
        /// <summary>
        /// The player carrying the ball during this segment
        /// (Can be computed from other properties in some implementations)
        /// </summary>
        Player BallCarrier { get; }

        /// <summary>
        /// Yards gained (or lost if negative) during this segment
        /// (Can be computed from other properties in some implementations)
        /// </summary>
        int YardsGained { get; }

        /// <summary>
        /// Whether this segment ended in a fumble
        /// </summary>
        bool EndedInFumble { get; set; }

        /// <summary>
        /// Player who fumbled (if EndedInFumble is true)
        /// </summary>
        Player? FumbledBy { get; set; }

        /// <summary>
        /// Player who recovered the fumble (if EndedInFumble is true)
        /// </summary>
        Player? RecoveredBy { get; set; }
    }
}
