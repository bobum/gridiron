namespace DomainObjects;

public class Division
{
    public int Id { get; set; }  // Primary key for EF Core
    public string Name { get; set; } = string.Empty;
    public int ConferenceId { get; set; }  // Foreign key
    public List<Team> Teams { get; set; } = new();  // EF Core navigation property
}
