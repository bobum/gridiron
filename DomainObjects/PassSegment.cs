namespace DomainObjects
{
    /// <summary>
    /// Represents a segment of a pass play (including laterals)
    /// </summary>
    public class PassSegment : IPlaySegment
    {
        public Player Passer { get; set; } = null!;
        public Player Receiver { get; set; } = null!;

        // IPlaySegment implementation
        public Player BallCarrier => Receiver;
        public int YardsGained => IsComplete ? AirYards + YardsAfterCatch : 0;
        public bool EndedInFumble { get; set; }
        public Player? FumbledBy { get; set; }
        public Player? RecoveredBy { get; set; }

        // Pass-specific properties
        public bool IsComplete { get; set; }
        public PassType Type { get; set; }
        public int AirYards { get; set; }  // Distance ball traveled in air
        public int YardsAfterCatch { get; set; }  // Yards gained after catch
    }

    public enum PassType
    {
        Forward,   // Legal forward pass (only one allowed per play)
        Lateral,   // Lateral/backward pass (unlimited)
        Backward,  // Intentional backward pass
        Short,     // Short forward pass
        Deep,      // Deep forward pass
        Screen     // Screen pass
    }
}
