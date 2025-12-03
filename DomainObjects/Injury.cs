namespace DomainObjects
{
    /// <summary>
    /// Represents an injury for runtime player state tracking.
    /// Not persisted to database - only used during game simulation.
    /// </summary>
    public class Injury
    {
        public InjuryType Type { get; set; }

        public InjurySeverity Severity { get; set; }

        public int PlaysUntilReturn { get; set; }
    }

    public enum InjuryType
    {
        None,
        Ankle,
        Knee,
        Shoulder,
        Concussion,
        Hamstring
    }

    public enum InjurySeverity
    {
        Minor,
        Moderate,
        GameEnding
    }
}
