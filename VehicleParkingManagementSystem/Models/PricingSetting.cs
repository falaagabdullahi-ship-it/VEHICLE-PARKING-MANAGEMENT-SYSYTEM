namespace VehicleParkingManagementSystem.Models;

/// <summary>Configurable hourly parking rate per vehicle type, editable via System Settings.</summary>
public class PricingSetting
{
    public int Id { get; set; }

    public VehicleType VehicleType { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal MinimumCharge { get; set; }
}
