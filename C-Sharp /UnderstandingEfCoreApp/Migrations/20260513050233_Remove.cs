using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnderstandingEfCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class Remove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "AccountNumber",
                keyValue: "0009998877",
                column: "LastAccessed",
                value: new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "customers",
                keyColumn: "Id",
                keyValue: 101,
                column: "DateOfBirth",
                value: new DateTime(2000, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "AccountNumber",
                keyValue: "0009998877",
                column: "LastAccessed",
                value: new DateTime(2026, 5, 13, 10, 28, 46, 320, DateTimeKind.Local).AddTicks(7040));

            migrationBuilder.UpdateData(
                table: "customers",
                keyColumn: "Id",
                keyValue: 101,
                column: "DateOfBirth",
                value: new DateTime(2026, 5, 13, 10, 28, 46, 290, DateTimeKind.Local).AddTicks(130));
        }
    }
}
