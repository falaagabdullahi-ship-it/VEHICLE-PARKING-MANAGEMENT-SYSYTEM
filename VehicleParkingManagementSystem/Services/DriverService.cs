using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Drivers;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _drivers;

    public DriverService(IDriverRepository drivers)
    {
        _drivers = drivers;
    }

    public async Task<PagedResult<Driver>> GetPagedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _drivers.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d =>
                d.FirstName.Contains(term) ||
                d.LastName.Contains(term) ||
                d.PhoneNumber.Contains(term) ||
                (d.Email != null && d.Email.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(d => d.FirstName).ThenBy(d => d.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Driver>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<Driver?> GetByIdAsync(int id) => _drivers.GetWithVehiclesAsync(id);

    public async Task<ServiceResult> CreateAsync(DriverFormViewModel model)
    {
        var driver = new Driver
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            LicenseNumber = string.IsNullOrWhiteSpace(model.LicenseNumber) ? null : model.LicenseNumber.Trim(),
            Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim()
        };

        await _drivers.AddAsync(driver);
        await _drivers.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(DriverFormViewModel model)
    {
        var driver = await _drivers.GetByIdAsync(model.Id);
        if (driver is null)
        {
            return ServiceResult.Failure("Driver not found.");
        }

        driver.FirstName = model.FirstName.Trim();
        driver.LastName = model.LastName.Trim();
        driver.PhoneNumber = model.PhoneNumber.Trim();
        driver.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        driver.LicenseNumber = string.IsNullOrWhiteSpace(model.LicenseNumber) ? null : model.LicenseNumber.Trim();
        driver.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        _drivers.Update(driver);
        await _drivers.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var driver = await _drivers.GetByIdAsync(id);
        if (driver is null)
        {
            return ServiceResult.Failure("Driver not found.");
        }

        try
        {
            _drivers.Remove(driver);
            await _drivers.SaveChangesAsync();
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This driver cannot be deleted because they still have registered vehicles.");
        }
    }
}
