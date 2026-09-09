using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingAreas;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class ParkingAreasController : Controller
{
    private const int PageSize = 10;

    private readonly IParkingAreaService _areaService;
    private readonly IDashboardService _dashboardService;
    private readonly ReportExportService _exportService;

    public ParkingAreasController(IParkingAreaService areaService, IDashboardService dashboardService, ReportExportService exportService)
    {
        _areaService = areaService;
        _dashboardService = dashboardService;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _areaService.GetPagedAsync(search, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        ViewBag.ParkingMap = await _dashboardService.GetParkingMapAsync();
        return View(result);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var result = await _areaService.GetPagedAsync(search, 1, int.MaxValue);
        var headers = new[] { "Name", "Location", "Description", "VIP" };
        var rows = result.Items.Select(a => new[]
        {
            a.Name, a.Location ?? "-", a.Description ?? "-", a.IsVip ? "Yes" : "No"
        });
        var excel = _exportService.ExportToExcel("Parking Areas", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"parking-areas-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public async Task<IActionResult> Details(int id)
    {
        var area = await _areaService.GetByIdAsync(id);
        if (area is null) return NotFound();
        return View(area);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Create() => View(new ParkingAreaFormViewModel());

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParkingAreaFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _areaService.CreateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["StatusMessage"] = "Parking area created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var area = await _areaService.GetByIdAsync(id);
        if (area is null) return NotFound();

        var model = new ParkingAreaFormViewModel
        {
            Id = area.Id,
            Name = area.Name,
            Location = area.Location,
            Description = area.Description,
            IsVip = area.IsVip
        };

        return View(model);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ParkingAreaFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var result = await _areaService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["StatusMessage"] = "Parking area updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var area = await _areaService.GetByIdAsync(id);
        if (area is null) return NotFound();
        return View(area);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _areaService.DeleteAsync(id);
        TempData["StatusMessage"] = result.Succeeded ? "Parking area deleted successfully." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
