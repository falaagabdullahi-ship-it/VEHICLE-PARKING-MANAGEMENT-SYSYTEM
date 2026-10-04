using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.Settings;

public class ParkingPackageRow
{
    public int Id { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Display only; null means the package is valid in any area.</summary>
    public string? ParkingAreaName { get; set; }

    [Range(0.1, 720)]
    public decimal DurationHours { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}
