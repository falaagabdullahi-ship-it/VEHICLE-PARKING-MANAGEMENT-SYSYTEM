using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleParkingManagementSystem.Models.ViewModels.Vehicles;

public class VehicleFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(20), Display(Name = "Vehicle Number")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Owner Name")]
    public string OwnerName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20), Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Vehicle Type")]
    public VehicleType VehicleType { get; set; }

    [Required, StringLength(100), Display(Name = "Vehicle Model")]
    public string VehicleModel { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Color { get; set; } = string.Empty;

    [Required, Display(Name = "Registration Date")]
    [DataType(DataType.Date)]
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow.Date;

    [Required, Display(Name = "Driver")]
    public int DriverId { get; set; }

    public IEnumerable<SelectListItem> DriverOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
