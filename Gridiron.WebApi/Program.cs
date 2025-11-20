using DataAccessLayer;
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

// Configure database
builder.Services.AddDbContext<GridironDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("GridironDb");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'GridironDb' not found");
    }
    options.UseSqlServer(connectionString);
});

// Register services
builder.Services.AddScoped<IGameSimulationService, GameSimulationService>();

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
