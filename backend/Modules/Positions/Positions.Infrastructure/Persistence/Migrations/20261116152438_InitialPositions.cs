using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Positions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "positions");

            migrationBuilder.CreateTable(
                name: "UserPositions",
                schema: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPositions_UserId",
                schema: "positions",
                table: "UserPositions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPositions",
                schema: "positions");
        }
    }
}
