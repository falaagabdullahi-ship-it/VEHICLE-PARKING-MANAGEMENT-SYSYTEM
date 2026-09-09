using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Drivers;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class DriversController : Controller
{
    private const int PageSize = 10;

    private readonly IDriverService _driverService;
    private readonly IAuditService _auditService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DriversController(
        IDriverService driverService,
        IAuditService auditService,
        ReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _driverService = driverService;
        _auditService = auditService;
        _exportService = exportService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _driverService.GetPagedAsync(search, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var result = await _driverService.GetPagedAsync(search, 1, int.MaxValue);
        var headers = new[] { "Name", "Phone", "Email", "License No.", "Registered" };
        var rows = result.Items.Select(d => new[]
        {
            d.FullName, d.PhoneNumber, d.Email ?? "-", d.LicenseNumber ?? "-", d.RegisteredDate.ToString("d MMM yyyy")
        });
        var excel = _exportService.ExportToExcel("Drivers", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"drivers-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public async Task<IActionResult> Details(int id)
    {
        var driver = await _driverService.GetByIdAsync(id);
        if (driver is null) return NotFound();
        return View(driver);
    }

    public IActionResult Create() => View(new DriverFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DriverFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _driverService.CreateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        await _auditService.LogAsync(_userManager.GetUserId(User), "Create", "Driver", details: $"{model.FirstName} {model.LastName}");
        TempData["StatusMessage"] = "Driver added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var driver = await _driverService.GetByIdAsync(id);
        if (driver is null) return NotFound();

        var model = new DriverFormViewModel
        {
            Id = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            PhoneNumber = driver.PhoneNumber,
            Email = driver.Email,
            LicenseNumber = driver.LicenseNumber,
            Address = driver.Address
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DriverFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var result = await _driverService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        await _auditService.LogAsync(_userManager.GetUserId(User), "Update", "Driver", id.ToString());
        TempData["StatusMessage"] = "Driver updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var driver = await _driverService.GetByIdAsync(id);
        if (driver is null) return NotFound();
        return View(driver);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _driverService.DeleteAsync(id);
        if (result.Succeeded)
        {
            await _auditService.LogAsync(_userManager.GetUserId(User), "Delete", "Driver", id.ToString());
        }
        TempData["StatusMessage"] = result.Succeeded ? "Driver deleted successfully." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
