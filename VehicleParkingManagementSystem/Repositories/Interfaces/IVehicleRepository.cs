using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Repositories.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByVehicleNumberAsync(string vehicleNumber);
    Task<Vehicle?> GetWithDetailsAsync(int id);
}
