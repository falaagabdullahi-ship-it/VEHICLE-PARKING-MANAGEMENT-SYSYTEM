using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Vehicles;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class VehiclesController : Controller
{
    private const int PageSize = 10;

    private readonly IVehicleService _vehicleService;
    private readonly IDriverRepository _driverRepository;
    private readonly IAuditService _auditService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public VehiclesController(
        IVehicleService vehicleService,
        IDriverRepository driverRepository,
        IAuditService auditService,
        ReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _vehicleService = vehicleService;
        _driverRepository = driverRepository;
        _auditService = auditService;
        _exportService = exportService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _vehicleService.GetPagedAsync(search, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var result = await _vehicleService.GetPagedAsync(search, 1, int.MaxValue);
        var headers = new[] { "Vehicle Number", "Owner", "Phone", "Type", "Model", "Color", "Registered" };
        var rows = result.Items.Select(v => new[]
        {
            v.VehicleNumber, v.OwnerName, v.PhoneNumber, v.VehicleType.ToString(), v.VehicleModel, v.Color,
            v.RegistrationDate.ToString("d MMM yyyy")
        });
        var excel = _exportService.ExportToExcel("Vehicles", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"vehicles-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public async Task<IActionResult> Details(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null) return NotFound();
        return View(vehicle);
    }

    public async Task<IActionResult> Create()
    {
        var model = new VehicleFormViewModel { DriverOptions = await GetDriverOptionsAsync() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.DriverOptions = await GetDriverOptionsAsync(model.DriverId);
            return View(model);
        }

        var result = await _vehicleService.CreateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.DriverOptions = await GetDriverOptionsAsync(model.DriverId);
            return View(model);
        }

        await _auditService.LogAsync(_userManager.GetUserId(User), "Create", "Vehicle", model.VehicleNumber);
        TempData["StatusMessage"] = "Vehicle registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null) return NotFound();

        var model = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            OwnerName = vehicle.OwnerName,
            PhoneNumber = vehicle.PhoneNumber,
            VehicleType = vehicle.VehicleType,
            VehicleModel = vehicle.VehicleModel,
            Color = vehicle.Color,
            RegistrationDate = vehicle.RegistrationDate,
            DriverId = vehicle.DriverId,
            DriverOptions = await GetDriverOptionsAsync(vehicle.DriverId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            model.DriverOptions = await GetDriverOptionsAsync(model.DriverId);
            return View(model);
        }

        var result = await _vehicleService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.DriverOptions = await GetDriverOptionsAsync(model.DriverId);
            return View(model);
        }

        await _auditService.LogAsync(_userManager.GetUserId(User), "Update", "Vehicle", model.Id.ToString());
        TempData["StatusMessage"] = "Vehicle updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null) return NotFound();
        return View(vehicle);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        var result = await _vehicleService.DeleteAsync(id);
        if (result.Succeeded)
        {
            var details = vehicle is null ? null : $"{vehicle.VehicleNumber} - {vehicle.OwnerName}";
            await _auditService.LogAsync(_userManager.GetUserId(User), "Delete", "Vehicle", id.ToString(), details: details);
        }
        TempData["StatusMessage"] = result.Succeeded
            ? "Vehicle deleted successfully."
            : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetDriverOptionsAsync(int? selectedId = null)
    {
        var drivers = await _driverRepository.Query()
            .OrderBy(d => d.FirstName).ThenBy(d => d.LastName)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.FirstName + " " + d.LastName + " (" + d.PhoneNumber + ")",
                Selected = selectedId.HasValue && d.Id == selectedId.Value
            })
            .ToListAsync();

        return drivers;
    }
}
