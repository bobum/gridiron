namespace DomainObjects
{
    /// <summary>
    /// Represents a segment of a return (punt/kickoff return)
    /// </summary>
    public class ReturnSegment : IPlaySegment
    {
        public Player BallCarrier { get; set; } = null!;
        public int YardsGained { get; set; }
        public bool EndedInFumble { get; set; }
        public Player? FumbledBy { get; set; }
        public Player? RecoveredBy { get; set; }
    }
}
