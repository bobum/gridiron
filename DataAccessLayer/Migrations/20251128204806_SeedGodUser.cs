using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class SeedGodUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed the initial God (Global Admin) user
            // This user has full access to all leagues, teams, and can assign roles
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "AzureAdObjectId", "Email", "DisplayName", "IsGlobalAdmin", "CreatedAt", "LastLoginAt", "IsDeleted" },
                values: new object[] {
                    "d01a7f27-fed3-4e5b-b6b6-f9de730e9fb5",  // Azure AD Object ID for scott@davisplanet.com
                    "scott@davisplanet.com",
                    "Scott Davis",
                    true,  // IsGlobalAdmin = true (God role)
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false  // IsDeleted = false
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seeded God user
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "AzureAdObjectId",
                keyValue: "d01a7f27-fed3-4e5b-b6b6-f9de730e9fb5");
        }
    }
}
