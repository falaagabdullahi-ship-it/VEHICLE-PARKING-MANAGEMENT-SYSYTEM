using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;

namespace VehicleParkingManagementSystem.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Vehicle?> GetByVehicleNumberAsync(string vehicleNumber) =>
        await DbSet.FirstOrDefaultAsync(v => v.VehicleNumber == vehicleNumber);

    public async Task<Vehicle?> GetWithDetailsAsync(int id) =>
        await DbSet.Include(v => v.Driver).FirstOrDefaultAsync(v => v.Id == id);
}
