using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;

namespace VehicleParkingManagementSystem.Repositories;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Driver?> GetWithVehiclesAsync(int id) =>
        await DbSet.Include(d => d.Vehicles).FirstOrDefaultAsync(d => d.Id == id);
}
