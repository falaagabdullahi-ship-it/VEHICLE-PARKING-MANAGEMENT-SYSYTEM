using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;

public class CheckInViewModel
{
    [Required, StringLength(20), Display(Name = "Vehicle Number")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Display(Name = "Parking Area (optional)")]
    public int? ParkingAreaId { get; set; }

    [Display(Name = "Package")]
    public int? ParkingPackageId { get; set; }

    public IEnumerable<SelectListItem> AreaOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<ParkingPackage> PackageOptions { get; set; } = Enumerable.Empty<ParkingPackage>();
}
