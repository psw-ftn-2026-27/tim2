using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShareLocationAcrossChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location_Longitude",
                schema: "games",
                table: "Quizzes",
                newName: "Location_Location_Longitude");

            migrationBuilder.RenameColumn(
                name: "Location_Latitude",
                schema: "games",
                table: "Quizzes",
                newName: "Location_Location_Latitude");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location_Location_Longitude",
                schema: "games",
                table: "Quizzes",
                newName: "Location_Longitude");

            migrationBuilder.RenameColumn(
                name: "Location_Location_Latitude",
                schema: "games",
                table: "Quizzes",
                newName: "Location_Latitude");
        }
    }
}
