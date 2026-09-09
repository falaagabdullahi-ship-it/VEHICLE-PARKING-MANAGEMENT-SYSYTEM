using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.Settings;

public class NewParkingPackageViewModel
{
    [Required]
    public VehicleType VehicleType { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, 720)]
    public decimal DurationHours { get; set; } = 1;

    [Range(0, 100000)]
    public decimal Price { get; set; }
}
