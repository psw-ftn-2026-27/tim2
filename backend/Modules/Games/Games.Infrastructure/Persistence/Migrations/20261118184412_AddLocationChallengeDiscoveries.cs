using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeDiscoveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "DiscoveredPointNumbers",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<int>(
                name: "EarnedXp",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SolvedAt",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscoveredPointNumbers",
                schema: "games",
                table: "LocationChallengeAttempts");

            migrationBuilder.DropColumn(
                name: "EarnedXp",
                schema: "games",
                table: "LocationChallengeAttempts");

            migrationBuilder.DropColumn(
                name: "SolvedAt",
                schema: "games",
                table: "LocationChallengeAttempts");
        }
    }
}
