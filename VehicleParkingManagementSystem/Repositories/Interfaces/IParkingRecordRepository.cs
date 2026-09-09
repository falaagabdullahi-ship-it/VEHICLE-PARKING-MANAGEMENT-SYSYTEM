using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;

namespace VehicleParkingManagementSystem.Repositories.Interfaces;

public interface IParkingRecordRepository : IRepository<ParkingRecord>
{
    Task<ParkingRecord?> GetActiveByVehicleNumberAsync(string vehicleNumber);
    Task<ParkingRecord?> GetWithDetailsAsync(int id);
    Task<PagedResult<ParkingRecord>> GetActiveSessionsPagedAsync(string? search, int pageNumber, int pageSize);
    Task<PagedResult<ParkingRecord>> GetByDatePagedAsync(DateTime date, string? search, int pageNumber, int pageSize);
}
