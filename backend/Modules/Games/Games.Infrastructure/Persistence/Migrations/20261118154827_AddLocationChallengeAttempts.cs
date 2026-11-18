using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                schema: "games",
                table: "LocationChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LocationChallengeAttempts",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    SearchRadiusInMeters = table.Column<int>(type: "integer", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeAttempts_ChallengeId_PlayerId_Round",
                schema: "games",
                table: "LocationChallengeAttempts",
                columns: new[] { "ChallengeId", "PlayerId", "Round" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationChallengeAttempts",
                schema: "games");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                schema: "games",
                table: "LocationChallenges");
        }
    }
}
