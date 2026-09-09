namespace VehicleParkingManagementSystem.Models;

/// <summary>A prepaid parking tier (fixed price for a fixed duration) offered for a given vehicle type.</summary>
public class ParkingPackage
{
    public int Id { get; set; }

    public VehicleType VehicleType { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
