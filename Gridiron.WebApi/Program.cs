using DataAccessLayer;
using DataAccessLayer.Repositories;
using Gridiron.WebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gridiron Football Simulation API",
        Version = "v1",
        Description = "REST API for running football game simulations and accessing team/player data"
    });
});

// ========================================
// DATABASE CONFIGURATION
// ========================================
// Configure database - ONLY accessed through repositories in DataAccessLayer
builder.Services.AddDbContext<GridironDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("GridironDb");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'GridironDb' not found");
    }
    options.UseSqlServer(connectionString);
});

// ========================================
// DATA ACCESS LAYER - Repository Pattern
// ========================================
// Register repositories - these are the ONLY way to access the database
// Controllers and services MUST use these repositories, NOT DbContext directly
builder.Services.AddScoped<ILeagueRepository, LeagueRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPlayerDataRepository, DatabasePlayerDataRepository>();

// ========================================
// APPLICATION SERVICES
// ========================================
builder.Services.AddScoped<IGameSimulationService, GameSimulationService>();

// ========================================
// GAME MANAGEMENT SERVICES
// ========================================
builder.Services.AddScoped<GameManagement.Services.ILeagueBuilderService, GameManagement.Services.LeagueBuilderService>();
builder.Services.AddScoped<GameManagement.Services.IPlayerGeneratorService, GameManagement.Services.PlayerGeneratorService>();
builder.Services.AddScoped<GameManagement.Services.ITeamBuilderService, GameManagement.Services.TeamBuilderService>();
builder.Services.AddScoped<GameManagement.Services.IPlayerProgressionService, GameManagement.Services.PlayerProgressionService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gridiron API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Display startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Gridiron Football Simulation API started");
logger.LogInformation("Swagger UI available at: {BaseUrl}", app.Environment.IsDevelopment() ? "http://localhost:5000" : "");

app.Run();
