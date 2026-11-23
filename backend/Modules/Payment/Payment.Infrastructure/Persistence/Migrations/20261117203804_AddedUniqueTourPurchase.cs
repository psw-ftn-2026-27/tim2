using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueTourPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourPurchaseTokens_BuyerId",
                schema: "payment",
                table: "TourPurchaseTokens");

            migrationBuilder.CreateIndex(
                name: "IX_TourPurchaseTokens_BuyerId_TourId",
                schema: "payment",
                table: "TourPurchaseTokens",
                columns: new[] { "BuyerId", "TourId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourPurchaseTokens_BuyerId_TourId",
                schema: "payment",
                table: "TourPurchaseTokens");

            migrationBuilder.CreateIndex(
                name: "IX_TourPurchaseTokens_BuyerId",
                schema: "payment",
                table: "TourPurchaseTokens",
                column: "BuyerId");
        }
    }
}
