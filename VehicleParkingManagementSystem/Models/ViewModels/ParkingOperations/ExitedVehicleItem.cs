namespace VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;

public class ExitedVehicleItem
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? AreaName { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime EventTime { get; set; }
    public decimal? Fee { get; set; }

    /// <summary>"Completed" for a normal check-out, "Deleted" when the vehicle record was removed.</summary>
    public string Status { get; set; } = string.Empty;

    public int? ParkingRecordId { get; set; }
}
