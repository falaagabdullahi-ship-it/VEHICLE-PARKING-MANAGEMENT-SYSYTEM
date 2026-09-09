namespace VehicleParkingManagementSystem.Models.ViewModels.Reports;

public class ParkingActivityReportRow
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string ParkingArea { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? Fee { get; set; }
}

public class ReservationReportRow
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string ParkingArea { get; set; } = string.Empty;
    public DateTime ReservedFrom { get; set; }
    public DateTime ReservedTo { get; set; }
    public ReservationStatus Status { get; set; }
}

public class VehicleReportRow
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string VehicleModel { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
}
