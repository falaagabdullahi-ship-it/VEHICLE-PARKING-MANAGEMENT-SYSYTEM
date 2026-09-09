using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Drivers;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IDriverService
{
    Task<PagedResult<Driver>> GetPagedAsync(string? search, int pageNumber, int pageSize);
    Task<Driver?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(DriverFormViewModel model);
    Task<ServiceResult> UpdateAsync(DriverFormViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
