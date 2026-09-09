using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.Settings;

public class PricingSettingRow
{
    public int Id { get; set; }
    public VehicleType VehicleType { get; set; }

    [Range(0, 100000)]
    public decimal HourlyRate { get; set; }

    [Range(0, 100000)]
    public decimal MinimumCharge { get; set; }
}
