using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Repositories.Interfaces;

namespace VehicleParkingManagementSystem.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Reservation?> GetWithDetailsAsync(int id) =>
        await DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Include(r => r.CreatedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<PagedResult<Reservation>> GetPagedAsync(string? search, string? userId, int pageNumber, int pageSize)
    {
        var query = DbSet
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.CreatedByUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => r.Vehicle!.VehicleNumber.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Reservation>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
