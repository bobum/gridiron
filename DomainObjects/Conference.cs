namespace DomainObjects;

public class Conference : SoftDeletableEntity
{
    public int Id { get; set; } // Primary key for EF Core

    public string Name { get; set; } = string.Empty;

    public int LeagueId { get; set; } // Foreign key

    public List<Division> Divisions { get; set; } = new ();  // EF Core navigation property
}
