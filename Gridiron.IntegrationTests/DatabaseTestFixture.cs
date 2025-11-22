using DataAccessLayer;
using DataAccessLayer.Repositories;
using DomainObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Test fixture that sets up an in-memory database for integration tests
/// Seeds the database with FirstNames, LastNames, and Colleges data
/// </summary>
public class DatabaseTestFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; private set; }
    public GridironDbContext DbContext { get; private set; }

    public DatabaseTestFixture()
    {
        // Create service collection and configure DI
        var services = new ServiceCollection();

        // Configure in-memory database with unique name per test run
        var dbName = $"GridironTestDb_{Guid.NewGuid()}";
        services.AddDbContext<GridironDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        // Register repositories
        services.AddScoped<ILeagueRepository, LeagueRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayByPlayRepository, PlayByPlayRepository>();
        services.AddScoped<IPlayerDataRepository, DatabasePlayerDataRepository>();

        // Register GameManagement services
        services.AddScoped<GameManagement.Services.ILeagueBuilderService, GameManagement.Services.LeagueBuilderService>();
        services.AddScoped<GameManagement.Services.ITeamBuilderService, GameManagement.Services.TeamBuilderService>();
        services.AddScoped<GameManagement.Services.IPlayerGeneratorService, GameManagement.Services.PlayerGeneratorService>();
        services.AddScoped<GameManagement.Services.IPlayerProgressionService, GameManagement.Services.PlayerProgressionService>();

        // Register WebApi services
        services.AddScoped<Gridiron.WebApi.Services.IGameSimulationService, Gridiron.WebApi.Services.GameSimulationService>();

        // Register logging
        services.AddLogging();

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();

        // Get DbContext instance
        DbContext = ServiceProvider.GetRequiredService<GridironDbContext>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();

        // Seed database with player generation data
        SeedDatabase().GetAwaiter().GetResult();
    }

    private async Task SeedDatabase()
    {
        // Load and seed FirstNames
        var firstNamesJson = await File.ReadAllTextAsync("SeedData/FirstNames.json");
        var firstNameStrings = JsonSerializer.Deserialize<List<string>>(firstNamesJson);
        if (firstNameStrings != null && firstNameStrings.Any())
        {
            var firstNames = firstNameStrings.Select(name => new FirstName { Name = name }).ToList();
            await DbContext.FirstNames.AddRangeAsync(firstNames);
        }

        // Load and seed LastNames
        var lastNamesJson = await File.ReadAllTextAsync("SeedData/LastNames.json");
        var lastNameStrings = JsonSerializer.Deserialize<List<string>>(lastNamesJson);
        if (lastNameStrings != null && lastNameStrings.Any())
        {
            var lastNames = lastNameStrings.Select(name => new LastName { Name = name }).ToList();
            await DbContext.LastNames.AddRangeAsync(lastNames);
        }

        // Load and seed Colleges
        var collegesJson = await File.ReadAllTextAsync("SeedData/Colleges.json");
        var collegeStrings = JsonSerializer.Deserialize<List<string>>(collegesJson);
        if (collegeStrings != null && collegeStrings.Any())
        {
            var colleges = collegeStrings.Select(college => new College { Name = college }).ToList();
            await DbContext.Colleges.AddRangeAsync(colleges);
        }

        // Save changes
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
    }
}
