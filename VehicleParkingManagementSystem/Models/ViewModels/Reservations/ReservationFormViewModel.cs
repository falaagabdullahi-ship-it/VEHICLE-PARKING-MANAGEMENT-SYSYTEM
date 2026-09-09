using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleParkingManagementSystem.Models.ViewModels.Reservations;

public class ReservationFormViewModel
{
    [Required, Display(Name = "Vehicle")]
    public int VehicleId { get; set; }

    [Display(Name = "Parking Area (optional)")]
    public int? ParkingAreaId { get; set; }

    [Required, Display(Name = "From")]
    [DataType(DataType.DateTime)]
    public DateTime ReservedFrom { get; set; } = DateTime.Now.AddMinutes(30);

    [Required, Display(Name = "To")]
    [DataType(DataType.DateTime)]
    public DateTime ReservedTo { get; set; } = DateTime.Now.AddHours(2);

    public IEnumerable<SelectListItem> VehicleOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> AreaOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
