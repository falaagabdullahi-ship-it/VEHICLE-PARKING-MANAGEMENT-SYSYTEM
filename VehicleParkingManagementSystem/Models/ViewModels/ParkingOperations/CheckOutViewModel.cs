namespace VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;

public class CheckOutViewModel
{
    public int ParkingRecordId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string? AreaName { get; set; }
    public DateTime CheckInTime { get; set; }
    public decimal EstimatedFee { get; set; }
    public string? PackageName { get; set; }
}
