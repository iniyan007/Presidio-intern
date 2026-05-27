using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnderstandingEfCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class CustomerWithAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_customers_CustomerId",
                table: "accounts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "customers",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastAccessed",
                table: "accounts",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.InsertData(
                table: "customers",
                columns: new[] { "Id", "DateOfBirth", "Email", "Name", "Phone", "Status" },
                values: new object[] { 101, new DateTime(2026, 5, 13, 10, 28, 46, 290, DateTimeKind.Local).AddTicks(130), "ramu@gmail.com", "Ramu", "9876543210", "Active" });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "AccountNumber", "Balance", "CustomerId", "LastAccessed", "Status" },
                values: new object[] { "0009998877", 134.3m, 101, new DateTime(2026, 5, 13, 10, 28, 46, 320, DateTimeKind.Local).AddTicks(7040), "Active" });

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Customer",
                table: "accounts",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Customer",
                table: "accounts");

            migrationBuilder.DeleteData(
                table: "accounts",
                keyColumn: "AccountNumber",
                keyValue: "0009998877");

            migrationBuilder.DeleteData(
                table: "customers",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "customers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastAccessed",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_customers_CustomerId",
                table: "accounts",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
