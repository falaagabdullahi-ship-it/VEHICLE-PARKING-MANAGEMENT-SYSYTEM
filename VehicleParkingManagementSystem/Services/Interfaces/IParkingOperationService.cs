using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IParkingOperationService
{
    Task<ServiceResult<ParkingRecord>> CheckInAsync(string vehicleNumber, int? parkingAreaId, int? parkingPackageId, string officerUserId);
    Task<ServiceResult<ParkingRecord>> CheckOutAsync(int parkingRecordId, string officerUserId);
    Task<ServiceResult<ParkingRecord>> CreateManualTransactionAsync(string vehicleNumber, int? parkingAreaId, DateTime checkInTime, DateTime? checkOutTime, decimal? fee, string officerUserId);
    Task<PagedResult<ParkingRecord>> GetActiveSessionsAsync(string? search, int pageNumber, int pageSize);
    Task<PagedResult<ParkingRecord>> GetByDateAsync(DateTime date, string? search, int pageNumber, int pageSize);
    Task<ParkingRecord?> GetDetailsAsync(int id);
    Task<decimal> PreviewFeeAsync(int parkingRecordId);

    /// <summary>Checks out any active session whose paid/reserved time has elapsed. Called periodically by the auto-checkout background service.</summary>
    Task ProcessExpiredPaidSessionsAsync();
}
