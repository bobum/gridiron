namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Play-by-play data DTO
/// </summary>
public class PlayDto
{
    public string PlayType { get; set; } = string.Empty;
    public string Possession { get; set; } = string.Empty;
    public int Down { get; set; }
    public int YardsToGo { get; set; }
    public int StartFieldPosition { get; set; }
    public int EndFieldPosition { get; set; }
    public int YardsGained { get; set; }
    public int StartTime { get; set; }
    public int StopTime { get; set; }
    public double ElapsedTime { get; set; }

    // Events
    public bool IsTouchdown { get; set; }
    public bool IsSafety { get; set; }
    public bool Interception { get; set; }
    public bool PossessionChange { get; set; }
    public List<string> Penalties { get; set; } = new();
    public List<string> Fumbles { get; set; } = new();
    public List<string> Injuries { get; set; } = new();

    // Description (play-by-play text)
    public string Description { get; set; } = string.Empty;
}
