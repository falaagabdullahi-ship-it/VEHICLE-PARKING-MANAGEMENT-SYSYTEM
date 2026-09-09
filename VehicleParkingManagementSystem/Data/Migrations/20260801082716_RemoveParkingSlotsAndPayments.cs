using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParkingManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParkingSlotsAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParkingAreaId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParkingAreaId",
                table: "ParkingRecords",
                type: "int",
                nullable: true);

            // Preserve the area each existing check-in/reservation belonged to before the slot table is dropped.
            migrationBuilder.Sql(
                "UPDATE r SET r.ParkingAreaId = s.ParkingAreaId FROM ParkingRecords r JOIN ParkingSlots s ON s.Id = r.ParkingSlotId;");
            migrationBuilder.Sql(
                "UPDATE r SET r.ParkingAreaId = s.ParkingAreaId FROM Reservations r JOIN ParkingSlots s ON s.Id = r.ParkingSlotId;");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingRecords_ParkingSlots_ParkingSlotId",
                table: "ParkingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "ParkingSlots");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ParkingSlotId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ParkingRecords_ParkingSlotId",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "ParkingRecords");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ParkingAreaId",
                table: "Reservations",
                column: "ParkingAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_ParkingAreaId",
                table: "ParkingRecords",
                column: "ParkingAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingRecords_ParkingAreas_ParkingAreaId",
                table: "ParkingRecords",
                column: "ParkingAreaId",
                principalTable: "ParkingAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingAreas_ParkingAreaId",
                table: "Reservations",
                column: "ParkingAreaId",
                principalTable: "ParkingAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingRecords_ParkingAreas_ParkingAreaId",
                table: "ParkingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingAreas_ParkingAreaId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ParkingAreaId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ParkingRecords_ParkingAreaId",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "ParkingAreaId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ParkingAreaId",
                table: "ParkingRecords");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "ParkingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ParkingSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingAreaId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    SlotNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSlots_ParkingAreas_ParkingAreaId",
                        column: x => x.ParkingAreaId,
                        principalTable: "ParkingAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingRecordId = table.Column<int>(type: "int", nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_ParkingRecords_ParkingRecordId",
                        column: x => x.ParkingRecordId,
                        principalTable: "ParkingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ParkingSlotId",
                table: "Reservations",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_ParkingSlotId",
                table: "ParkingRecords",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlots_ParkingAreaId_SlotNumber",
                table: "ParkingSlots",
                columns: new[] { "ParkingAreaId", "SlotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ParkingRecordId",
                table: "Payments",
                column: "ParkingRecordId",
                unique: true,
                filter: "[ParkingRecordId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProcessedByUserId",
                table: "Payments",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReservationId",
                table: "Payments",
                column: "ReservationId",
                unique: true,
                filter: "[ReservationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingRecords_ParkingSlots_ParkingSlotId",
                table: "ParkingRecords",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
