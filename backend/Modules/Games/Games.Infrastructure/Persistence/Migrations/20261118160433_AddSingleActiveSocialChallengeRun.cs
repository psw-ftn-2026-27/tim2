using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSingleActiveSocialChallengeRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeRuns_ChallengeId",
                schema: "games",
                table: "SocialChallengeRuns",
                column: "ChallengeId",
                unique: true,
                filter: "\"Status\" = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialChallengeRuns_ChallengeId",
                schema: "games",
                table: "SocialChallengeRuns");
        }
    }
}
