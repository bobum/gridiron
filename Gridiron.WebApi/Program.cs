using DataAccessLayer;
using DataAccessLayer.Repositories;
using DataAccessLayer.SeedData;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

// Check if we're running in seed mode
if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    await SeedDataRunner.RunAsync(args);
    return; // Exit after seeding
}

var builder = WebApplication.CreateBuilder(args);

// ========================================
// AUTHENTICATION & AUTHORIZATION
// ========================================
// Check if we're running in E2E test mode (for CI/CD)
var isE2ETestMode = builder.Configuration.GetValue<bool>("E2ETestMode", false) ||
                    Environment.GetEnvironmentVariable("E2E_TEST_MODE") == "true";

if (isE2ETestMode)
{
    Console.WriteLine("[E2E Test Mode] Running with authentication DISABLED for E2E tests");
    // In E2E test mode, add authentication/authorization services but configure them to allow anonymous access
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true) // Always succeed - bypass all authorization
            .Build();
    });
}
else
{
    // Production mode - require valid JWT tokens
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    builder.Services.AddAuthorization();
}

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

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
builder.Services.AddScoped<IConferenceRepository, ConferenceRepository>();
builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPlayByPlayRepository, PlayByPlayRepository>();
builder.Services.AddScoped<IPlayerDataRepository, DatabasePlayerDataRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();

// ========================================
// APPLICATION SERVICES
// ========================================
builder.Services.AddScoped<IGameSimulationService, GameSimulationService>();
builder.Services.AddScoped<IGridironAuthorizationService, GridironAuthorizationService>();

// ========================================
// GAME MANAGEMENT SERVICES
// ========================================
builder.Services.AddScoped<GameManagement.Services.ILeagueBuilderService, GameManagement.Services.LeagueBuilderService>();
builder.Services.AddScoped<GameManagement.Services.IConferenceBuilderService, GameManagement.Services.ConferenceBuilderService>();
builder.Services.AddScoped<GameManagement.Services.IDivisionBuilderService, GameManagement.Services.DivisionBuilderService>();
builder.Services.AddScoped<GameManagement.Services.ITeamBuilderService, GameManagement.Services.TeamBuilderService>();
builder.Services.AddScoped<GameManagement.Services.IPlayerGeneratorService, GameManagement.Services.PlayerGeneratorService>();
builder.Services.AddScoped<GameManagement.Services.IPlayerProgressionService, GameManagement.Services.PlayerProgressionService>();
builder.Services.AddScoped<GameManagement.Services.IScheduleGeneratorService, GameManagement.Services.ScheduleGeneratorService>();

// ========================================
// GRIDIRON ENGINE SERVICES (from NuGet package)
// ========================================
builder.Services.AddSingleton<Gridiron.Engine.Api.IGameEngine, Gridiron.Engine.Api.GameEngine>();
builder.Services.AddSingleton<GameManagement.Mapping.GridironMapper>();
builder.Services.AddScoped<GameManagement.Services.IEngineSimulationService, GameManagement.Services.EngineSimulationService>();

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
        options.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger (frees up root for React frontend)
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Display startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Gridiron Football Simulation API started");
logger.LogInformation("Swagger UI available at: {BaseUrl}", app.Environment.IsDevelopment() ? "http://localhost:5000/swagger" : "");

app.Run();
