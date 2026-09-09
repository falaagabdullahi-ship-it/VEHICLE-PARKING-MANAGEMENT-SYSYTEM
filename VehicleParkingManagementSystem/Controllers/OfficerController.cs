using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = AppRoles.ParkingOfficer)]
public class OfficerController : Controller
{
    private readonly IDashboardService _dashboardService;

    public OfficerController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetOperationalDashboardAsync();
        return View(model);
    }
}
