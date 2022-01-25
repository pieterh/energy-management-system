using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DataStore.Migrations
{
    public partial class SomeDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "EnergyDelivered",
                table: "ChargingTransactions",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "CostDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnergyDelivered = table.Column<double>(type: "REAL", nullable: false),
                    Cost = table.Column<double>(type: "REAL", nullable: false),
                    TarifStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TarifUsage = table.Column<double>(type: "REAL", nullable: false),
                    ChargingTransactionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostDetail", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CostDetail_ChargingTransactions_ChargingTransactionID",
                        column: x => x.ChargingTransactionID,
                        principalTable: "ChargingTransactions",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostDetail_ChargingTransactionID",
                table: "CostDetail",
                column: "ChargingTransactionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostDetail");

            migrationBuilder.AlterColumn<int>(
                name: "EnergyDelivered",
                table: "ChargingTransactions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
