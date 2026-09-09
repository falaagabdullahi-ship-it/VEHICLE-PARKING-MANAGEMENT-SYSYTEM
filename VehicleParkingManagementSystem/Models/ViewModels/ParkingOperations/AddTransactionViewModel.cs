using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;

public class AddTransactionViewModel
{
    [Required, StringLength(20), Display(Name = "Vehicle Number")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Display(Name = "Parking Area (optional)")]
    public int? ParkingAreaId { get; set; }

    [Required, Display(Name = "Check-In Time")]
    [DataType(DataType.DateTime)]
    public DateTime CheckInTime { get; set; } = DateTime.Now;

    [Display(Name = "Check-Out Time (optional)")]
    [DataType(DataType.DateTime)]
    public DateTime? CheckOutTime { get; set; }

    [Display(Name = "Amount (optional, auto-calculated if left blank and checked out)")]
    [Range(0, 1_000_000)]
    public decimal? Fee { get; set; }

    public IEnumerable<SelectListItem> AreaOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
