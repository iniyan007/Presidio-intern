using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Bookings",
                newName: "TotalPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFee",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PlatformFee",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Bookings",
                newName: "Price");
        }
    }
}
