namespace VehicleParkingManagementSystem.Models;

/// <summary>A prepaid parking tier (fixed price for a fixed duration) offered for a given vehicle type.</summary>
public class ParkingPackage
{
    public int Id { get; set; }

    public VehicleType VehicleType { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>The parking area this package is sold for, or null when it is valid in any area.</summary>
    public int? ParkingAreaId { get; set; }

    public ParkingArea? ParkingArea { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
