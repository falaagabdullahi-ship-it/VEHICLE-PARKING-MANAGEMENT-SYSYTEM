using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;

namespace VehicleParkingManagementSystem.Repositories;

public class ParkingAreaRepository : Repository<ParkingArea>, IParkingAreaRepository
{
    public ParkingAreaRepository(AppDbContext db) : base(db)
    {
    }
}
