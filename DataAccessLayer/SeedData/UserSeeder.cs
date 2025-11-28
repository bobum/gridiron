using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds the initial God (Global Admin) user for the Gridiron system.
    /// This user has full access to all leagues, teams, and can assign roles.
    /// </summary>
    public static class UserSeeder
    {
        /// <summary>
        /// Seeds the God user if not already present in the database.
        /// This is idempotent - safe to run multiple times.
        /// </summary>
        public static async Task SeedGodUserAsync(GridironDbContext db)
        {
            const string godUserAzureAdObjectId = "d01a7f27-fed3-4e5b-b6b6-f9de730e9fb5";

            // Check if God user already exists
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.AzureAdObjectId == godUserAzureAdObjectId);

            if (existingUser != null)
            {
                // Ensure they have God privileges (in case it was removed)
                if (!existingUser.IsGlobalAdmin)
                {
                    existingUser.IsGlobalAdmin = true;
                    await db.SaveChangesAsync();
                    Console.WriteLine("  - Updated existing user to God status");
                }
                else
                {
                    Console.WriteLine("  - God user already exists");
                }
                return;
            }

            // Create the God user
            var godUser = new User
            {
                AzureAdObjectId = godUserAzureAdObjectId,
                Email = "scott@davisplanet.com",
                DisplayName = "Scott Davis",
                IsGlobalAdmin = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Users.Add(godUser);
            await db.SaveChangesAsync();

            Console.WriteLine("  - God user created: scott@davisplanet.com");
        }
    }
}
