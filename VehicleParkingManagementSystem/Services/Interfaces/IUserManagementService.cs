using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Users;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IUserManagementService
{
    Task<PagedResult<StaffUserRow>> GetPagedAsync(string? search, int pageNumber, int pageSize);
    Task<ServiceResult> CreateStaffUserAsync(CreateStaffUserViewModel model);
    Task<EditUserViewModel?> GetForEditAsync(string userId);
    Task<ServiceResult> UpdateUserAsync(EditUserViewModel model, string currentUserId);
    Task<ServiceResult> ToggleLockoutAsync(string userId, string currentUserId);
}
