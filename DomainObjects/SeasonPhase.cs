namespace DomainObjects;

/// <summary>
/// Represents the different phases a season can be in.
/// </summary>
public enum SeasonPhase
{
    /// <summary>
    /// Pre-season phase - exhibition games, roster cuts, practice
    /// </summary>
    Preseason,

    /// <summary>
    /// Regular season phase - scheduled league games
    /// </summary>
    RegularSeason,

    /// <summary>
    /// Playoff phase - elimination tournament
    /// </summary>
    Playoffs,

    /// <summary>
    /// Off-season phase - draft, free agency, roster management
    /// </summary>
    Offseason
}
