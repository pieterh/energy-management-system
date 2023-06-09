using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DataStore.Migrations
{
    /// <inheritdoc />
    public partial class ChargingTransactionStartEnd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "ChargingTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "ChargingTransactions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "End",
                table: "ChargingTransactions");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "ChargingTransactions");
        }
    }
}
