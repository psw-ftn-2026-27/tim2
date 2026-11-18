using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialChallengeRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialChallengeRuns",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EarnedXp = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialChallengeParticipants",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SocialChallengeRunId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialChallengeParticipants_SocialChallengeRuns_SocialChall~",
                        column: x => x.SocialChallengeRunId,
                        principalSchema: "games",
                        principalTable: "SocialChallengeRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeRunId",
                schema: "games",
                table: "SocialChallengeParticipants",
                column: "SocialChallengeRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialChallengeParticipants",
                schema: "games");

            migrationBuilder.DropTable(
                name: "SocialChallengeRuns",
                schema: "games");
        }
    }
}
