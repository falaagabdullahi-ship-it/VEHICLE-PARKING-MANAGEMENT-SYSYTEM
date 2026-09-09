using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Vehicles;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IVehicleService
{
    Task<PagedResult<Vehicle>> GetPagedAsync(string? search, int pageNumber, int pageSize);
    Task<Vehicle?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(VehicleFormViewModel model);
    Task<ServiceResult> UpdateAsync(VehicleFormViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
