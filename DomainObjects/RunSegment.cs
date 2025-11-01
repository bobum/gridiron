namespace DomainObjects
{
    /// <summary>
    /// Represents a segment of a run play
    /// </summary>
    public class RunSegment : IPlaySegment
    {
        public Player BallCarrier { get; set; } = null!;
        public int YardsGained { get; set; }
        public bool EndedInFumble { get; set; }
        public Player? FumbledBy { get; set; }
        public Player? RecoveredBy { get; set; }

        // Run-specific properties
        public RunDirection Direction { get; set; }
    }

    public enum RunDirection
    {
        Left,
        Right,
        Middle,
        MiddleLeft,
        MiddleRight,
        UpTheMiddle,
        OffLeftTackle,
        OffRightTackle,
        Sweep
    }
}
