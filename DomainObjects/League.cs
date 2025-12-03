namespace DomainObjects;

public class League : SoftDeletableEntity
{
    public int Id { get; set; } // Primary key for EF Core

    public string Name { get; set; } = string.Empty;

    public List<Conference> Conferences { get; set; } = new ();  // EF Core navigation property

    /// <summary>
    /// Gets or sets legacy season year field - consider using CurrentSeason.Year instead.
    /// </summary>
    public int Season { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets foreign key to the current active season (nullable if no season started).
    /// </summary>
    public int? CurrentSeasonId { get; set; }

    /// <summary>
    /// Gets or sets navigation property to the current active season.
    /// </summary>
    public Season? CurrentSeason { get; set; }

    /// <summary>
    /// Gets or sets all seasons associated with this league.
    /// </summary>
    public List<Season> Seasons { get; set; } = new ();
}
