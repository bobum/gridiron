namespace DomainObjects;

public class League
{
    public int Id { get; set; }  // Primary key for EF Core
    public string Name { get; set; } = string.Empty;
    public List<Conference> Conferences { get; set; } = new();  // EF Core navigation property
    public int Season { get; set; }
    public bool IsActive { get; set; }
}
