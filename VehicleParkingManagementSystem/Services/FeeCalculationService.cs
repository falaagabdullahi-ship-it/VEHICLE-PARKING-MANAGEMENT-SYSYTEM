using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class FeeCalculationService : IFeeCalculationService
{
    private readonly IRepository<PricingSetting> _pricingSettings;

    public FeeCalculationService(IRepository<PricingSetting> pricingSettings)
    {
        _pricingSettings = pricingSettings;
    }

    public async Task<decimal> CalculateFeeAsync(VehicleType vehicleType, TimeSpan duration)
    {
        var pricing = await _pricingSettings.Query()
            .FirstOrDefaultAsync(p => p.VehicleType == vehicleType);

        // Fallback rate if no pricing row exists for this vehicle type.
        var hourlyRate = pricing?.HourlyRate ?? 100m;
        var minimumCharge = pricing?.MinimumCharge ?? 50m;

        // Billable hours round up, so any partial hour counts as a full hour.
        var billableHours = Math.Max(1, (int)Math.Ceiling(duration.TotalHours));
        var fee = billableHours * hourlyRate;

        return Math.Max(fee, minimumCharge);
    }
}
