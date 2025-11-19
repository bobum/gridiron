using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    /// <summary>
    /// Design-time factory for creating GridironDbContext instances during migrations
    /// This allows EF Core tools to create the DbContext without running the application
    /// </summary>
    public class GridironDbContextFactory : IDesignTimeDbContextFactory<GridironDbContext>
    {
        public GridironDbContext CreateDbContext(string[] args)
        {
            // Build configuration to read connection string
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<GridironDbContextFactory>(optional: true)  // For local development
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("GridironDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'GridironDb' not found. " +
                    "Please configure it in appsettings.json, user secrets, or environment variables."
                );
            }

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<GridironDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new GridironDbContext(optionsBuilder.Options);
        }
    }
}
