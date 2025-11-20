using System.ComponentModel.DataAnnotations;

namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Request DTO for simulating a game
/// </summary>
public class SimulateGameRequest
{
    [Required]
    public int HomeTeamId { get; set; }

    [Required]
    public int AwayTeamId { get; set; }

    /// <summary>
    /// Optional seed for reproducible simulations
    /// </summary>
    public int? RandomSeed { get; set; }
}
