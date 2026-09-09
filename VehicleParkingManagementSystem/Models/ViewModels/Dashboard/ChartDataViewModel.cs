namespace VehicleParkingManagementSystem.Models.ViewModels.Dashboard;

public class ChartDataViewModel
{
    public List<string> Labels { get; set; } = [];
    public List<decimal> Data { get; set; } = [];
}

public class DriverDashboardViewModel
{
    public int VehicleCount { get; set; }
    public bool HasActiveSession { get; set; }
    public string? ActiveVehicleNumber { get; set; }
    public string? ActiveAreaName { get; set; }
    public DateTime? ActiveCheckInTime { get; set; }
    public int UpcomingReservationCount { get; set; }
    public decimal TotalSpent { get; set; }
}
