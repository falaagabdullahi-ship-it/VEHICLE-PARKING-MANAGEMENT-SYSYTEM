using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Repositories.Interfaces;

public interface IDriverRepository : IRepository<Driver>
{
    Task<Driver?> GetWithVehiclesAsync(int id);
}
