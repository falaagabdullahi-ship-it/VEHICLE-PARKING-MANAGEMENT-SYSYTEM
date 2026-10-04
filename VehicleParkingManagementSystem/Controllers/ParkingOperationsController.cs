using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class ParkingOperationsController : Controller
{
    private const int PageSize = 10;

    private readonly IParkingOperationService _operations;
    private readonly IDashboardService _dashboardService;
    private readonly IParkingAreaRepository _areaRepository;
    private readonly IRepository<ParkingPackage> _packageRepository;
    private readonly IAuditService _auditService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ParkingOperationsController(
        IParkingOperationService operations,
        IDashboardService dashboardService,
        IParkingAreaRepository areaRepository,
        IRepository<ParkingPackage> packageRepository,
        IAuditService auditService,
        ReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _operations = operations;
        _dashboardService = dashboardService;
        _areaRepository = areaRepository;
        _packageRepository = packageRepository;
        _auditService = auditService;
        _exportService = exportService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var sessions = await _operations.GetActiveSessionsAsync(search, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        ViewBag.CheckIn = new CheckInViewModel
        {
            AreaOptions = await GetAreaOptionsAsync(),
            PackageOptions = await _packageRepository.Query()
                .Include(p => p.ParkingArea)
                .Where(p => p.IsActive)
                .OrderBy(p => p.VehicleType).ThenBy(p => p.DurationMinutes)
                .ToListAsync()
        };
        return View(sessions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Please enter a valid vehicle number.";
            return RedirectToAction(nameof(Index));
        }

        var officerId = _userManager.GetUserId(User)!;
        var result = await _operations.CheckInAsync(model.VehicleNumber, model.ParkingAreaId, model.ParkingPackageId, officerId);

        if (result.Succeeded)
        {
            await _auditService.LogAsync(officerId, "CheckIn", "ParkingRecord", result.Value!.Id.ToString(), details: model.VehicleNumber.ToUpperInvariant());
        }

        TempData["StatusMessage"] = result.Succeeded
            ? $"Vehicle {model.VehicleNumber.ToUpperInvariant()} checked in successfully."
            : result.Error;

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CheckOut(int id)
    {
        var record = await _operations.GetDetailsAsync(id);
        if (record is null || record.CheckOutTime is not null) return NotFound();

        var model = new CheckOutViewModel
        {
            ParkingRecordId = record.Id,
            VehicleNumber = record.Vehicle!.VehicleNumber,
            AreaName = record.ParkingArea?.Name,
            CheckInTime = record.CheckInTime,
            EstimatedFee = await _operations.PreviewFeeAsync(id),
            PackageName = record.ParkingPackage?.Name
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.EstimatedFee = await _operations.PreviewFeeAsync(model.ParkingRecordId);
            return View(model);
        }

        var officerId = _userManager.GetUserId(User)!;
        var result = await _operations.CheckOutAsync(model.ParkingRecordId, officerId);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.EstimatedFee = await _operations.PreviewFeeAsync(model.ParkingRecordId);
            return View(model);
        }

        await _auditService.LogAsync(officerId, "CheckOut", "ParkingRecord", result.Value!.Id.ToString(), details: $"Fee: {result.Value.Fee:C}");
        return RedirectToAction(nameof(Receipt), new { id = result.Value!.Id });
    }

    public async Task<IActionResult> AddTransaction()
    {
        var model = new AddTransactionViewModel { AreaOptions = await GetAreaOptionsAsync() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTransaction(AddTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AreaOptions = await GetAreaOptionsAsync();
            return View(model);
        }

        var officerId = _userManager.GetUserId(User)!;
        var result = await _operations.CreateManualTransactionAsync(
            model.VehicleNumber, model.ParkingAreaId, model.CheckInTime, model.CheckOutTime, model.Fee, officerId);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.AreaOptions = await GetAreaOptionsAsync();
            return View(model);
        }

        await _auditService.LogAsync(officerId, "AddTransaction", "ParkingRecord", result.Value!.Id.ToString(), details: model.VehicleNumber.ToUpperInvariant());
        TempData["StatusMessage"] = $"Transaction for {model.VehicleNumber.ToUpperInvariant()} added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> History(DateTime? date, string? search, string mode = "entered", int page = 1)
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;
        ViewBag.Date = targetDate.ToString("yyyy-MM-dd");
        ViewBag.Search = search;

        if (mode == "exited")
        {
            var exited = await _dashboardService.GetExitedActivityAsync(targetDate, search, page < 1 ? 1 : page, PageSize);
            ViewBag.Mode = "exited";
            return View("ExitedHistory", exited);
        }

        var records = await _operations.GetByDateAsync(targetDate, search, page < 1 ? 1 : page, PageSize);
        ViewBag.Mode = "entered";
        return View(records);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var sessions = await _operations.GetActiveSessionsAsync(search, 1, int.MaxValue);
        var headers = new[] { "Vehicle", "Owner", "Area", "Check-In Time" };
        var rows = sessions.Items.Select(r => new[]
        {
            r.Vehicle?.VehicleNumber ?? "-", r.Vehicle?.OwnerName ?? "-",
            r.ParkingArea?.Name ?? "-", r.CheckInTime.ToLocalTime().ToString("d MMM yyyy HH:mm")
        });
        var excel = _exportService.ExportToExcel("Active Transactions", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"transactions-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public async Task<IActionResult> HistoryExportExcel(DateTime? date, string? search, string mode = "entered")
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;
        var headers = new[] { "Vehicle", "Owner", "Area", "Check-In", "Check-Out", "Fee", "Status" };

        if (mode == "exited")
        {
            var exited = await _dashboardService.GetExitedActivityAsync(targetDate, search, 1, int.MaxValue);
            var exitedRows = exited.Items.Select(i => new[]
            {
                i.VehicleNumber, i.OwnerName, i.AreaName ?? "-",
                i.CheckInTime?.ToLocalTime().ToString("d MMM yyyy HH:mm") ?? "-",
                i.EventTime.ToLocalTime().ToString("d MMM yyyy HH:mm"),
                i.Fee?.ToString("F2") ?? "-",
                i.Status
            });
            var exitedExcel = _exportService.ExportToExcel("Vehicles Exited", headers, exitedRows);
            return File(exitedExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"vehicles-exited-{targetDate:yyyyMMdd}.xlsx");
        }

        var records = await _operations.GetByDateAsync(targetDate, search, 1, int.MaxValue);
        var rows = records.Items.Select(r => new[]
        {
            r.Vehicle?.VehicleNumber ?? "-", r.Vehicle?.OwnerName ?? "-",
            r.ParkingArea?.Name ?? "-",
            r.CheckInTime.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.CheckOutTime?.ToLocalTime().ToString("d MMM yyyy HH:mm") ?? "-",
            r.Fee?.ToString("F2") ?? "-",
            r.CheckOutTime is null ? "Active" : "Completed"
        });
        var excel = _exportService.ExportToExcel("Vehicles By Date", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"vehicles-{targetDate:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var record = await _operations.GetDetailsAsync(id);
        if (record is null || record.CheckOutTime is null) return NotFound();
        return View(record);
    }

    private async Task<List<SelectListItem>> GetAreaOptionsAsync()
    {
        return await _areaRepository.Query()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            .ToListAsync();
    }
}
