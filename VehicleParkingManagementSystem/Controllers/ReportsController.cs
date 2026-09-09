using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ParkingOfficer}")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ReportExportService _exportService;
    private readonly IParkingAreaRepository _areaRepository;
    private readonly IDriverRepository _driverRepository;

    public ReportsController(
        IReportService reportService,
        ReportExportService exportService,
        IParkingAreaRepository areaRepository,
        IDriverRepository driverRepository)
    {
        _reportService = reportService;
        _exportService = exportService;
        _areaRepository = areaRepository;
        _driverRepository = driverRepository;
    }

    public IActionResult Index() => View();

    // ---------- Parking Activity (Daily / Weekly / Monthly / Usage) ----------

    public async Task<IActionResult> ParkingActivity(DateTime? from, DateTime? to, int? parkingAreaId, string? vehicleNumber)
    {
        var rows = await _reportService.GetParkingActivityAsync(from, to, parkingAreaId, vehicleNumber);
        ViewBag.AreaOptions = await GetAreaOptionsAsync(parkingAreaId);
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.VehicleNumber = vehicleNumber;
        return View(rows);
    }

    public async Task<IActionResult> ParkingActivityExportPdf(DateTime? from, DateTime? to, int? parkingAreaId, string? vehicleNumber)
    {
        var rows = await _reportService.GetParkingActivityAsync(from, to, parkingAreaId, vehicleNumber);
        var headers = new[] { "Vehicle", "Owner", "Area", "Check-In", "Check-Out", "Fee" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.OwnerName, r.ParkingArea,
            r.CheckInTime.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.CheckOutTime?.ToLocalTime().ToString("d MMM yyyy HH:mm") ?? "-",
            r.Fee?.ToString("C") ?? "-"
        });
        var pdf = _exportService.ExportToPdf("Parking Activity Report", headers, data);
        return File(pdf, "application/pdf", $"parking-activity-{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }

    public async Task<IActionResult> ParkingActivityExportExcel(DateTime? from, DateTime? to, int? parkingAreaId, string? vehicleNumber)
    {
        var rows = await _reportService.GetParkingActivityAsync(from, to, parkingAreaId, vehicleNumber);
        var headers = new[] { "Vehicle", "Owner", "Area", "Check-In", "Check-Out", "Fee" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.OwnerName, r.ParkingArea,
            r.CheckInTime.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.CheckOutTime?.ToLocalTime().ToString("d MMM yyyy HH:mm") ?? "-",
            r.Fee?.ToString("F2") ?? "-"
        });
        var excel = _exportService.ExportToExcel("Parking Activity", headers, data);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"parking-activity-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    // ---------- Reservations ----------

    public async Task<IActionResult> Reservations(DateTime? from, DateTime? to, ReservationStatus? status)
    {
        var rows = await _reportService.GetReservationsAsync(from, to, status);
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        return View(rows);
    }

    public async Task<IActionResult> ReservationsExportPdf(DateTime? from, DateTime? to, ReservationStatus? status)
    {
        var rows = await _reportService.GetReservationsAsync(from, to, status);
        var headers = new[] { "Vehicle", "Area", "From", "To", "Status" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.ParkingArea,
            r.ReservedFrom.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.ReservedTo.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.Status.ToString()
        });
        var pdf = _exportService.ExportToPdf("Reservation Report", headers, data);
        return File(pdf, "application/pdf", $"reservations-{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }

    public async Task<IActionResult> ReservationsExportExcel(DateTime? from, DateTime? to, ReservationStatus? status)
    {
        var rows = await _reportService.GetReservationsAsync(from, to, status);
        var headers = new[] { "Vehicle", "Area", "From", "To", "Status" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.ParkingArea,
            r.ReservedFrom.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.ReservedTo.ToLocalTime().ToString("d MMM yyyy HH:mm"),
            r.Status.ToString()
        });
        var excel = _exportService.ExportToExcel("Reservations", headers, data);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reservations-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    // ---------- Vehicles ----------

    public async Task<IActionResult> Vehicles(int? driverId)
    {
        var rows = await _reportService.GetVehiclesAsync(driverId);
        ViewBag.DriverOptions = await GetDriverOptionsAsync(driverId);
        return View(rows);
    }

    public async Task<IActionResult> VehiclesExportPdf(int? driverId)
    {
        var rows = await _reportService.GetVehiclesAsync(driverId);
        var headers = new[] { "Vehicle Number", "Owner", "Driver", "Type", "Model", "Registered" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.OwnerName, r.DriverName, r.VehicleType.ToString(), r.VehicleModel,
            r.RegistrationDate.ToString("d MMM yyyy")
        });
        var pdf = _exportService.ExportToPdf("Vehicle Report", headers, data);
        return File(pdf, "application/pdf", $"vehicles-{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }

    public async Task<IActionResult> VehiclesExportExcel(int? driverId)
    {
        var rows = await _reportService.GetVehiclesAsync(driverId);
        var headers = new[] { "Vehicle Number", "Owner", "Driver", "Type", "Model", "Registered" };
        var data = rows.Select(r => new[]
        {
            r.VehicleNumber, r.OwnerName, r.DriverName, r.VehicleType.ToString(), r.VehicleModel,
            r.RegistrationDate.ToString("d MMM yyyy")
        });
        var excel = _exportService.ExportToExcel("Vehicles", headers, data);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"vehicles-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    private async Task<List<SelectListItem>> GetAreaOptionsAsync(int? selectedId)
    {
        return await _areaRepository.Query()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name, Selected = a.Id == selectedId })
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> GetDriverOptionsAsync(int? selectedId)
    {
        return await _driverRepository.Query()
            .OrderBy(d => d.FirstName)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FirstName + " " + d.LastName, Selected = d.Id == selectedId })
            .ToListAsync();
    }
}
