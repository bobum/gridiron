using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WonCoinToss",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "DeferredPossession",
                table: "Games",
                newName: "IsComplete");

            migrationBuilder.AddColumn<int>(
                name: "CurrentSeasonId",
                table: "Leagues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayedAt",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonWeekId",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CurrentWeek = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegularSeasonWeeks = table.Column<int>(type: "int", nullable: false),
                    ChampionTeamId = table.Column<int>(type: "int", nullable: true),
                    PlayoffTeamIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seasons_Teams_ChampionTeamId",
                        column: x => x.ChampionTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SeasonWeeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonWeeks_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CurrentSeasonId",
                table: "Leagues",
                column: "CurrentSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SeasonWeekId",
                table: "Games",
                column: "SeasonWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_ChampionTeamId",
                table: "Seasons",
                column: "ChampionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId",
                table: "Seasons",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeeks_SeasonId_WeekNumber",
                table: "SeasonWeeks",
                columns: new[] { "SeasonId", "WeekNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_Games_SeasonWeeks_SeasonWeekId",
                table: "Games",
                column: "SeasonWeekId",
                principalTable: "SeasonWeeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leagues_Seasons_CurrentSeasonId",
                table: "Leagues",
                column: "CurrentSeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_SeasonWeeks_SeasonWeekId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Leagues_Seasons_CurrentSeasonId",
                table: "Leagues");

            migrationBuilder.DropTable(
                name: "SeasonWeeks");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Leagues_CurrentSeasonId",
                table: "Leagues");

            migrationBuilder.DropIndex(
                name: "IX_Games_SeasonWeekId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CurrentSeasonId",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "PlayedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SeasonWeekId",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "IsComplete",
                table: "Games",
                newName: "DeferredPossession");

            migrationBuilder.AddColumn<int>(
                name: "WonCoinToss",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
