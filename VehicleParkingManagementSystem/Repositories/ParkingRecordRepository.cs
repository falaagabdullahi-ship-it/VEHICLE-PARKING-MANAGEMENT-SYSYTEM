using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Repositories.Interfaces;

namespace VehicleParkingManagementSystem.Repositories;

public class ParkingRecordRepository : Repository<ParkingRecord>, IParkingRecordRepository
{
    public ParkingRecordRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<ParkingRecord?> GetActiveByVehicleNumberAsync(string vehicleNumber) =>
        await DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => r.CheckOutTime == null && r.Vehicle!.VehicleNumber == vehicleNumber)
            .FirstOrDefaultAsync();

    public async Task<ParkingRecord?> GetWithDetailsAsync(int id) =>
        await DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Include(r => r.CheckedInByUser)
            .Include(r => r.CheckedOutByUser)
            .Include(r => r.ParkingPackage)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<PagedResult<ParkingRecord>> GetActiveSessionsPagedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => r.CheckOutTime == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                r.Vehicle!.VehicleNumber.Contains(term) ||
                r.Vehicle.OwnerName.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CheckInTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ParkingRecord>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ParkingRecord>> GetByDatePagedAsync(DateTime date, string? search, int pageNumber, int pageSize)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var query = DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => r.CheckInTime >= dayStart && r.CheckInTime < dayEnd)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                r.Vehicle!.VehicleNumber.Contains(term) ||
                r.Vehicle.OwnerName.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CheckInTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ParkingRecord>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
