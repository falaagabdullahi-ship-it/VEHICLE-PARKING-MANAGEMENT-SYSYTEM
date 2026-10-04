using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Users;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<StaffUserRow>> GetPagedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Email!.Contains(term) ||
                u.UserName!.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = new List<StaffUserRow>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new StaffUserRow
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = roles.ToList(),
                IsLockedOut = await _userManager.IsLockedOutAsync(user)
            });
        }

        return new PagedResult<StaffUserRow>
        {
            Items = rows,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceResult> CreateStaffUserAsync(CreateStaffUserViewModel model)
    {
        if (model.Role is not (AppRoles.Admin or AppRoles.ParkingOfficer or AppRoles.Driver))
        {
            return ServiceResult.Failure("Invalid role selected.");
        }

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing is not null)
        {
            return ServiceResult.Failure("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        return ServiceResult.Success();
    }

    public async Task<EditUserViewModel?> GetForEditAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName ?? string.Empty,
            Role = roles.FirstOrDefault() ?? AppRoles.Driver
        };
    }

    public async Task<ServiceResult> UpdateUserAsync(EditUserViewModel model, string currentUserId)
    {
        if (!AppRoles.All.Contains(model.Role))
        {
            return ServiceResult.Failure("Invalid role selected.");
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        if (user.Id == currentUserId && model.Role != AppRoles.Admin)
        {
            return ServiceResult.Failure("You cannot remove the Admin role from your own account.");
        }

        var userName = model.UserName.Trim();
        if (!string.Equals(user.UserName, userName, StringComparison.Ordinal))
        {
            var renamed = await _userManager.SetUserNameAsync(user, userName);
            if (!renamed.Succeeded)
            {
                return ServiceResult.Failure(string.Join(" ", renamed.Errors.Select(e => e.Description)));
            }
        }

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!reset.Succeeded)
            {
                return ServiceResult.Failure(string.Join(" ", reset.Errors.Select(e => e.Description)));
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count != 1 || currentRoles[0] != model.Role)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ToggleLockoutAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId)
        {
            return ServiceResult.Failure("You cannot lock your own account.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var isLockedOut = await _userManager.IsLockedOutAsync(user);

        if (isLockedOut)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        return ServiceResult.Success();
    }
}
