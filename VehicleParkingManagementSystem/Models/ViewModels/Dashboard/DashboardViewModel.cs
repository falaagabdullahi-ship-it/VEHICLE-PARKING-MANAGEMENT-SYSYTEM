namespace VehicleParkingManagementSystem.Models.ViewModels.Dashboard;

public class DashboardViewModel
{
    public int ActiveSessions { get; set; }
    public int VehiclesToday { get; set; }
    public int VehiclesExitedToday { get; set; }
    public int TotalRegisteredVehicles { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }

    /// <summary>Percentage change vs yesterday's check-ins; null when yesterday had zero to compare against.</summary>
    public double? VehiclesTodayTrendPct { get; set; }

    /// <summary>Percentage change vs yesterday's check-outs; null when yesterday had zero to compare against.</summary>
    public double? VehiclesExitedTodayTrendPct { get; set; }

    /// <summary>Percentage change vs last month's revenue; null when last month had zero to compare against.</summary>
    public double? RevenueTrendPct { get; set; }

    public List<RecentActivityItem> RecentActivities { get; set; } = [];
    public List<ParkingMapAreaViewModel> ParkingMap { get; set; } = [];
}

public class ParkingMapAreaViewModel
{
    public int? AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public bool IsVip { get; set; }
    public int ActiveCount { get; set; }
    public List<string> VehicleNumbers { get; set; } = [];
}

public class RecentActivityItem
{
    public int RecordId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string ActivityLabel { get; set; } = string.Empty;
    public string? VehicleNumber { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "bi-info-circle";
}
