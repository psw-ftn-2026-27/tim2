using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSocialChallengeParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeRunId",
                schema: "games",
                table: "SocialChallengeParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeRunId_PlayerId",
                schema: "games",
                table: "SocialChallengeParticipants",
                columns: new[] { "SocialChallengeRunId", "PlayerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeRunId_PlayerId",
                schema: "games",
                table: "SocialChallengeParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeRunId",
                schema: "games",
                table: "SocialChallengeParticipants",
                column: "SocialChallengeRunId");
        }
    }
}
