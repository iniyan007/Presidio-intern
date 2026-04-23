using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Trips");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Trips",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_BusId",
                table: "Trips",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_RouteId",
                table: "Trips",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Buses_BusId",
                table: "Trips",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Routes_RouteId",
                table: "Trips",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Buses_BusId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Routes_RouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_BusId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_RouteId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Trips");

            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Trips",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
