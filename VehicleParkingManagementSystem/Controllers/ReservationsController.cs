using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Reservations;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private const int PageSize = 10;

    private readonly IReservationService _reservationService;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IParkingAreaRepository _areaRepository;
    private readonly IAuditService _auditService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservationsController(
        IReservationService reservationService,
        IVehicleRepository vehicleRepository,
        IParkingAreaRepository areaRepository,
        IAuditService auditService,
        ReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _reservationService = reservationService;
        _vehicleRepository = vehicleRepository;
        _areaRepository = areaRepository;
        _auditService = auditService;
        _exportService = exportService;
        _userManager = userManager;
    }

    private bool IsStaff => User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.ParkingOfficer);

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var scopedUserId = IsStaff ? null : userId;
        var result = await _reservationService.GetPagedAsync(search, scopedUserId, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var userId = _userManager.GetUserId(User)!;
        var scopedUserId = IsStaff ? null : userId;
        var result = await _reservationService.GetPagedAsync(search, scopedUserId, 1, int.MaxValue);

        var headers = new[] { "Vehicle", "Area", "From", "To", "Status" };
        var rows = result.Items.Select(r => new[]
        {
            r.Vehicle?.VehicleNumber ?? "-", r.ParkingArea?.Name ?? "-",
            r.ReservedFrom.ToLocalTime().ToString("d MMM yyyy HH:mm"), r.ReservedTo.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.Status.ToString()
        });
        var excel = _exportService.ExportToExcel("Reservations", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reservations-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public async Task<IActionResult> Create()
    {
        var model = new ReservationFormViewModel
        {
            VehicleOptions = await GetVehicleOptionsAsync(),
            AreaOptions = await GetAreaOptionsAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.VehicleOptions = await GetVehicleOptionsAsync();
            model.AreaOptions = await GetAreaOptionsAsync();
            return View(model);
        }

        if (!IsStaff && !await IsOwnVehicleAsync(model.VehicleId))
        {
            return Forbid();
        }

        var userId = _userManager.GetUserId(User)!;
        var fromUtc = DateTime.SpecifyKind(model.ReservedFrom, DateTimeKind.Local).ToUniversalTime();
        var toUtc = DateTime.SpecifyKind(model.ReservedTo, DateTimeKind.Local).ToUniversalTime();

        var result = await _reservationService.CreateAsync(model.VehicleId, model.ParkingAreaId, fromUtc, toUtc, userId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.VehicleOptions = await GetVehicleOptionsAsync();
            model.AreaOptions = await GetAreaOptionsAsync();
            return View(model);
        }

        await _auditService.LogAsync(userId, "Create", "Reservation", result.Value!.Id.ToString());
        TempData["StatusMessage"] = "Reservation confirmed.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetDetailsAsync(id);
        if (reservation is null) return NotFound();

        if (!IsStaff && reservation.CreatedByUserId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var reservation = await _reservationService.GetDetailsAsync(id);
        if (reservation is null) return NotFound();

        if (!IsStaff && reservation.CreatedByUserId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        var result = await _reservationService.CancelAsync(id, reason);
        if (result.Succeeded)
        {
            await _auditService.LogAsync(_userManager.GetUserId(User), "Cancel", "Reservation", id.ToString(), reason);
        }
        TempData["StatusMessage"] = result.Succeeded ? "Reservation cancelled." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _reservationService.GetDetailsAsync(id);
        if (reservation is null) return NotFound();
        return View(reservation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reservation = await _reservationService.GetDetailsAsync(id);
        var result = await _reservationService.DeleteAsync(id);
        if (result.Succeeded)
        {
            var details = reservation?.Vehicle is null ? null : reservation.Vehicle.VehicleNumber;
            await _auditService.LogAsync(_userManager.GetUserId(User), "Delete", "Reservation", id.ToString(), details: details);
        }
        TempData["StatusMessage"] = result.Succeeded ? "Reservation deleted successfully." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> IsOwnVehicleAsync(int vehicleId)
    {
        var userId = _userManager.GetUserId(User);
        var vehicle = await _vehicleRepository.GetWithDetailsAsync(vehicleId);
        return vehicle?.Driver?.ApplicationUserId == userId;
    }

    private async Task<List<SelectListItem>> GetVehicleOptionsAsync()
    {
        var query = _vehicleRepository.Query().Include(v => v.Driver).AsQueryable();

        if (!IsStaff)
        {
            var userId = _userManager.GetUserId(User);
            query = query.Where(v => v.Driver!.ApplicationUserId == userId);
        }

        return await query
            .OrderBy(v => v.VehicleNumber)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.VehicleNumber + " - " + v.VehicleModel
            })
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> GetAreaOptionsAsync()
    {
        return await _areaRepository.Query()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            .ToListAsync();
    }
}
