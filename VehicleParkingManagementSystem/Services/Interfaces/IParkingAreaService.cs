using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingAreas;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IParkingAreaService
{
    Task<PagedResult<ParkingArea>> GetPagedAsync(string? search, int pageNumber, int pageSize);
    Task<ParkingArea?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(ParkingAreaFormViewModel model);
    Task<ServiceResult> UpdateAsync(ParkingAreaFormViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
