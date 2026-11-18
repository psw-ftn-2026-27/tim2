using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedCartPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AdminId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "payment",
                table: "Transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourPurchaseTokens",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TourId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPurchaseTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourPurchaseTokens_BuyerId",
                schema: "payment",
                table: "TourPurchaseTokens",
                column: "BuyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourPurchaseTokens",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "payment",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "payment",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "AdminId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
