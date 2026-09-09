using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.ParkingAreas;

public class ParkingAreaFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "VIP Area")]
    public bool IsVip { get; set; }
}
