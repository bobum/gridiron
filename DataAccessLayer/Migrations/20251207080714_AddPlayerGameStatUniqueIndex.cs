using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerGameStatUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerGameStats_PlayerId",
                table: "PlayerGameStats");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStats_PlayerId_GameId",
                table: "PlayerGameStats",
                columns: new[] { "PlayerId", "GameId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerGameStats_PlayerId_GameId",
                table: "PlayerGameStats");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStats_PlayerId",
                table: "PlayerGameStats",
                column: "PlayerId");
        }
    }
}
