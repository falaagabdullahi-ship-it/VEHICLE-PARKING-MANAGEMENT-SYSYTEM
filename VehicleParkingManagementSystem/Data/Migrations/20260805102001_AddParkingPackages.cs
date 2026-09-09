using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParkingManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoCheckedOut",
                table: "ParkingRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidUntil",
                table: "ParkingRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParkingPackageId",
                table: "ParkingRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParkingPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingPackages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_ParkingPackageId",
                table: "ParkingRecords",
                column: "ParkingPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingRecords_ParkingPackages_ParkingPackageId",
                table: "ParkingRecords",
                column: "ParkingPackageId",
                principalTable: "ParkingPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingRecords_ParkingPackages_ParkingPackageId",
                table: "ParkingRecords");

            migrationBuilder.DropTable(
                name: "ParkingPackages");

            migrationBuilder.DropIndex(
                name: "IX_ParkingRecords_ParkingPackageId",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "AutoCheckedOut",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "PaidUntil",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "ParkingPackageId",
                table: "ParkingRecords");
        }
    }
}
