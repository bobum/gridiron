using DomainObjects;
using GameManagement.Helpers;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for handling player progression, aging, and retirement
/// </summary>
public class PlayerProgressionService : IPlayerProgressionService
{
    private readonly ILogger<PlayerProgressionService> _logger;
    private readonly Random _random;

    public PlayerProgressionService(ILogger<PlayerProgressionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    public bool AgePlayerOneYear(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        var previousAge = player.Age;
        player.Age++;
        player.Exp++;

        _logger.LogDebug("Aging player {FirstName} {LastName} from {PreviousAge} to {NewAge} (Exp: {Exp})",
            player.FirstName, player.LastName, previousAge, player.Age, player.Exp);

        // Apply age curve adjustments based on age bracket
        ApplyAgeCurveAdjustments(player, previousAge);

        // Check if player should retire
        if (ShouldRetire(player))
        {
            _logger.LogInformation("Player {FirstName} {LastName} ({Position}, Age {Age}) is retiring",
                player.FirstName, player.LastName, player.Position, player.Age);
            return false;  // Player retired
        }

        return true;  // Player is still active
    }

    public int CalculateOverallRating(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        return OverallRatingCalculator.Calculate(player);
    }

    public bool ShouldRetire(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        // Forced retirement at age 40+
        if (player.Age >= 40)
        {
            _logger.LogInformation("Forced retirement: {FirstName} {LastName} reached age {Age}",
                player.FirstName, player.LastName, player.Age);
            return true;
        }

        // No retirement before age 30
        if (player.Age < 30)
        {
            return false;
        }

        // Calculate retirement probability based on age
        var retirementProbability = CalculateRetirementProbability(player);

        // Roll dice
        var roll = _random.NextDouble();

        if (roll < retirementProbability)
        {
            _logger.LogInformation("Random retirement: {FirstName} {LastName} (Age {Age}, Overall {Overall}, Probability {Probability:P0}, Roll {Roll:P0})",
                player.FirstName, player.LastName, player.Age, CalculateOverallRating(player), retirementProbability, roll);
            return true;
        }

        return false;
    }

    // Private helper methods

    private void ApplyAgeCurveAdjustments(Player player, int previousAge)
    {
        // Age curves based on NFL data
        // Ages 22-26: Development phase (slight improvements)
        // Ages 27-30: Peak performance (minimal changes)
        // Ages 31-34: Decline phase (physical attributes decrease)
        // Ages 35+: Rapid decline (significant decreases)

        if (player.Age >= 22 && player.Age <= 26)
        {
            // Development phase: +1 to +3 improvement on key skills
            ApplyDevelopmentBonus(player);
        }
        else if (player.Age >= 27 && player.Age <= 30)
        {
            // Peak phase: No change or slight +1 to awareness/discipline
            ApplyPeakMaintenance(player);
        }
        else if (player.Age >= 31 && player.Age <= 34)
        {
            // Decline phase: -1 to -3 on physical attributes
            ApplyDeclinePhase(player);
        }
        else if (player.Age >= 35)
        {
            // Rapid decline: -3 to -5 on physical attributes
            ApplyRapidDecline(player);
        }
    }

    private void ApplyDevelopmentBonus(Player player)
    {
        // Young players improve their skills
        var improvement = _random.Next(1, 4);  // +1 to +3

        // Improve position-specific skills (limited by potential)
        switch (player.Position)
        {
            case Positions.QB:
                player.Passing = Math.Min(player.Potential, player.Passing + improvement);
                player.Awareness = Math.Min(player.Potential, player.Awareness + improvement / 2);
                break;

            case Positions.RB:
                player.Rushing = Math.Min(player.Potential, player.Rushing + improvement);
                player.Agility = Math.Min(player.Potential, player.Agility + improvement / 2);
                break;

            case Positions.WR:
                player.Catching = Math.Min(player.Potential, player.Catching + improvement);
                player.Speed = Math.Min(player.Potential, player.Speed + improvement / 2);
                break;

            case Positions.TE:
                player.Catching = Math.Min(player.Potential, player.Catching + improvement);
                player.Blocking = Math.Min(player.Potential, player.Blocking + improvement / 2);
                break;

            case Positions.C:
            case Positions.G:
            case Positions.T:
                player.Blocking = Math.Min(player.Potential, player.Blocking + improvement);
                player.Strength = Math.Min(player.Potential, player.Strength + improvement / 2);
                break;

            case Positions.DE:
            case Positions.DT:
                player.Tackling = Math.Min(player.Potential, player.Tackling + improvement);
                player.Strength = Math.Min(player.Potential, player.Strength + improvement / 2);
                break;

            case Positions.LB:
            case Positions.OLB:
                player.Tackling = Math.Min(player.Potential, player.Tackling + improvement);
                player.Coverage = Math.Min(player.Potential, player.Coverage + improvement / 2);
                break;

            case Positions.CB:
                player.Coverage = Math.Min(player.Potential, player.Coverage + improvement);
                player.Speed = Math.Min(player.Potential, player.Speed + improvement / 2);
                break;

            case Positions.S:
            case Positions.FS:
                player.Coverage = Math.Min(player.Potential, player.Coverage + improvement);
                player.Tackling = Math.Min(player.Potential, player.Tackling + improvement / 2);
                break;

            case Positions.K:
            case Positions.P:
                player.Kicking = Math.Min(player.Potential, player.Kicking + improvement);
                break;
        }

        _logger.LogDebug("Development bonus applied: {FirstName} {LastName} improved by +{Improvement}",
            player.FirstName, player.LastName, improvement);
    }

    private void ApplyPeakMaintenance(Player player)
    {
        // Peak years: minimal changes, slight increase to mental attributes
        var mentalBonus = _random.Next(0, 2);  // 0 or +1

        player.Awareness = Math.Min(99, player.Awareness + mentalBonus);
        player.Discipline = Math.Min(99, player.Discipline + mentalBonus);

        _logger.LogDebug("Peak maintenance: {FirstName} {LastName} gained +{Bonus} to mental attributes",
            player.FirstName, player.LastName, mentalBonus);
    }

    private void ApplyDeclinePhase(Player player)
    {
        // Physical decline: -1 to -3 on Speed, Agility, Strength
        var decline = _random.Next(1, 4);  // -1 to -3

        player.Speed = Math.Max(40, player.Speed - decline);
        player.Agility = Math.Max(40, player.Agility - decline);
        player.Strength = Math.Max(40, player.Strength - decline / 2);

        // Position-specific adjustments
        if (IsSpeedPosition(player.Position))
        {
            // Speed positions decline more in Speed/Agility
            player.Speed = Math.Max(40, player.Speed - decline / 2);
            player.Agility = Math.Max(40, player.Agility - decline / 2);
        }

        _logger.LogDebug("Decline phase: {FirstName} {LastName} lost -{Decline} to physical attributes",
            player.FirstName, player.LastName, decline);
    }

    private void ApplyRapidDecline(Player player)
    {
        // Rapid decline: -3 to -5 on physical attributes
        var decline = _random.Next(3, 6);  // -3 to -5

        player.Speed = Math.Max(30, player.Speed - decline);
        player.Agility = Math.Max(30, player.Agility - decline);
        player.Strength = Math.Max(30, player.Strength - decline);

        // All skills decline
        player.Passing = Math.Max(30, player.Passing - decline / 2);
        player.Catching = Math.Max(30, player.Catching - decline / 2);
        player.Rushing = Math.Max(30, player.Rushing - decline / 2);
        player.Blocking = Math.Max(30, player.Blocking - decline / 2);
        player.Tackling = Math.Max(30, player.Tackling - decline / 2);
        player.Coverage = Math.Max(30, player.Coverage - decline / 2);
        player.Kicking = Math.Max(30, player.Kicking - decline / 2);

        _logger.LogWarning("Rapid decline: {FirstName} {LastName} (Age {Age}) lost -{Decline} across all attributes",
            player.FirstName, player.LastName, player.Age, decline);
    }

    private double CalculateRetirementProbability(Player player)
    {
        var baseProbability = 0.0;

        // Age-based probability
        if (player.Age >= 30 && player.Age < 33)
            baseProbability = 0.02;  // 2% per year
        else if (player.Age >= 33 && player.Age < 35)
            baseProbability = 0.05;  // 5% per year
        else if (player.Age >= 35 && player.Age < 37)
            baseProbability = 0.15;  // 15% per year
        else if (player.Age >= 37 && player.Age < 39)
            baseProbability = 0.30;  // 30% per year
        else if (player.Age >= 39)
            baseProbability = 0.50;  // 50% per year

        // Performance modifier: Low-rated players retire earlier
        var overall = CalculateOverallRating(player);
        if (overall < 50)
            baseProbability += 0.20;  // +20% if poor performance
        else if (overall < 60)
            baseProbability += 0.10;  // +10% if below average
        else if (overall >= 80)
            baseProbability -= 0.05;  // -5% if elite (stars play longer)

        // Injury modifier: High fragility increases retirement chance
        if (player.Fragility > 70)
            baseProbability += 0.10;  // +10% if injury-prone

        // Health modifier: Injured players more likely to retire
        if (player.IsInjured)
            baseProbability += 0.05;  // +5% if currently injured

        // Clamp between 0 and 0.95 (never 100% before age 40)
        return Math.Clamp(baseProbability, 0.0, 0.95);
    }

    private bool IsSpeedPosition(Positions position)
    {
        return position switch
        {
            Positions.WR => true,
            Positions.RB => true,
            Positions.CB => true,
            Positions.S => true,
            Positions.FS => true,
            _ => false
        };
    }
}
