using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Dashboard;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetOperationalDashboardAsync();
    Task<DriverDashboardViewModel> GetDriverDashboardAsync(string applicationUserId);

    Task<ChartDataViewModel> GetDailyVisitorsChartAsync(int days = 7);
    Task<ChartDataViewModel> GetWeeklyRevenueChartAsync(int days = 7);
    Task<ChartDataViewModel> GetMonthlyRevenueChartAsync(int months = 6);
    Task<ChartDataViewModel> GetVehicleTypeChartAsync();
    Task<ChartDataViewModel> GetPeakHoursChartAsync();
    Task<List<ParkingMapAreaViewModel>> GetParkingMapAsync();

    /// <summary>Vehicles that checked out or were deleted on the given date, merged into one list.</summary>
    Task<PagedResult<ExitedVehicleItem>> GetExitedActivityAsync(DateTime date, string? search, int pageNumber, int pageSize);
}
