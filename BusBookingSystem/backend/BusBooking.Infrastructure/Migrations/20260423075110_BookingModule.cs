using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BookingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_SeatLocks_TripId_SeatNumber",
                table: "SeatLocks");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "SeatLocks");

            migrationBuilder.RenameColumn(
                name: "LockedByUserId",
                table: "SeatLocks",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ExpiryTime",
                table: "SeatLocks",
                newName: "LockedAt");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Bookings",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "SeatId",
                table: "SeatLocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SeatId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusId = table.Column<int>(type: "integer", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    IsWindow = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatLocks_TripId_SeatId",
                table: "SeatLocks",
                columns: new[] { "TripId", "SeatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_BusId",
                table: "Seats",
                column: "BusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_SeatLocks_TripId_SeatId",
                table: "SeatLocks");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "SeatLocks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SeatLocks",
                newName: "LockedByUserId");

            migrationBuilder.RenameColumn(
                name: "LockedAt",
                table: "SeatLocks",
                newName: "ExpiryTime");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Bookings",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<string>(
                name: "SeatNumber",
                table: "SeatLocks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SeatLocks_TripId_SeatNumber",
                table: "SeatLocks",
                columns: new[] { "TripId", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
