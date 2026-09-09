using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingAreas;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class ParkingAreaService : IParkingAreaService
{
    private readonly IParkingAreaRepository _areas;

    public ParkingAreaService(IParkingAreaRepository areas)
    {
        _areas = areas;
    }

    public async Task<PagedResult<ParkingArea>> GetPagedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _areas.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => a.Name.Contains(term) || (a.Location != null && a.Location.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ParkingArea>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<ParkingArea?> GetByIdAsync(int id) => _areas.GetByIdAsync(id);

    public async Task<ServiceResult> CreateAsync(ParkingAreaFormViewModel model)
    {
        if (await _areas.ExistsAsync(a => a.Name == model.Name))
        {
            return ServiceResult.Failure("A parking area with this name already exists.");
        }

        var area = new ParkingArea
        {
            Name = model.Name.Trim(),
            Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            IsVip = model.IsVip
        };

        await _areas.AddAsync(area);
        await _areas.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ParkingAreaFormViewModel model)
    {
        var area = await _areas.GetByIdAsync(model.Id);
        if (area is null)
        {
            return ServiceResult.Failure("Parking area not found.");
        }

        if (await _areas.ExistsAsync(a => a.Name == model.Name && a.Id != model.Id))
        {
            return ServiceResult.Failure("A parking area with this name already exists.");
        }

        area.Name = model.Name.Trim();
        area.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();
        area.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        area.IsVip = model.IsVip;

        _areas.Update(area);
        await _areas.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var area = await _areas.GetByIdAsync(id);
        if (area is null)
        {
            return ServiceResult.Failure("Parking area not found.");
        }

        try
        {
            _areas.Remove(area);
            await _areas.SaveChangesAsync();
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This parking area cannot be deleted because it has parking or reservation history.");
        }
    }
}
