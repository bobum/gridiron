namespace DomainObjects;

/// <summary>
/// Entity for last names used in player generation.
/// </summary>
public class LastName
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
