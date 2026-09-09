using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> DailyVisitorsChart() => Json(await _dashboardService.GetDailyVisitorsChartAsync());

    [HttpGet]
    public async Task<IActionResult> WeeklyRevenueChart() => Json(await _dashboardService.GetWeeklyRevenueChartAsync());

    [HttpGet]
    public async Task<IActionResult> MonthlyRevenueChart() => Json(await _dashboardService.GetMonthlyRevenueChartAsync());

    [HttpGet]
    public async Task<IActionResult> VehicleTypeChart() => Json(await _dashboardService.GetVehicleTypeChartAsync());

    [HttpGet]
    public async Task<IActionResult> PeakHoursChart() => Json(await _dashboardService.GetPeakHoursChartAsync());

    [HttpGet]
    public async Task<IActionResult> ParkingMap() => Json(await _dashboardService.GetParkingMapAsync());
}
