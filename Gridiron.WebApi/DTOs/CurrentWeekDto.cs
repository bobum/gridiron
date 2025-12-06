using DomainObjects;

namespace Gridiron.WebApi.DTOs;

public class CurrentWeekDto
{
    public int WeekNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public List<GameDto> Games { get; set; } = new();
}