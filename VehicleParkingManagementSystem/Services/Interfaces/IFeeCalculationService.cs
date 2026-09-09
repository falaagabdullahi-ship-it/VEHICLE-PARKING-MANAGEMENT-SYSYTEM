using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IFeeCalculationService
{
    Task<decimal> CalculateFeeAsync(VehicleType vehicleType, TimeSpan duration);
}
