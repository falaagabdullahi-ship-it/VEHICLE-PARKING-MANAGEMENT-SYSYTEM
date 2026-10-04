using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParkingManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingAreaToPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParkingAreaId",
                table: "ParkingPackages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingPackages_ParkingAreaId",
                table: "ParkingPackages",
                column: "ParkingAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingPackages_ParkingAreas_ParkingAreaId",
                table: "ParkingPackages",
                column: "ParkingAreaId",
                principalTable: "ParkingAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingPackages_ParkingAreas_ParkingAreaId",
                table: "ParkingPackages");

            migrationBuilder.DropIndex(
                name: "IX_ParkingPackages_ParkingAreaId",
                table: "ParkingPackages");

            migrationBuilder.DropColumn(
                name: "ParkingAreaId",
                table: "ParkingPackages");
        }
    }
}
