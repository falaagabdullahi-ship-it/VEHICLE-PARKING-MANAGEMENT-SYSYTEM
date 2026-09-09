using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Reports;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IReportService
{
    Task<List<ParkingActivityReportRow>> GetParkingActivityAsync(DateTime? from, DateTime? to, int? parkingAreaId, string? vehicleNumber);
    Task<List<ReservationReportRow>> GetReservationsAsync(DateTime? from, DateTime? to, ReservationStatus? status);
    Task<List<VehicleReportRow>> GetVehiclesAsync(int? driverId);
}
