using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LayoutId",
                table: "Buses",
                newName: "TotalSeats");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Buses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Buses");

            migrationBuilder.RenameColumn(
                name: "TotalSeats",
                table: "Buses",
                newName: "LayoutId");
        }
    }
}
