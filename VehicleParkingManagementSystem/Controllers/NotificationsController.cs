using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(INotificationService notificationService, ReportExportService exportService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _exportService = exportService;
        _userManager = userManager;
    }

    private bool IsStaff => User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.ParkingOfficer);

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var notifications = await _notificationService.GetRecentAsync(userId, IsStaff, take: 50);
        return View(notifications);
    }

    public async Task<IActionResult> ExportExcel()
    {
        var userId = _userManager.GetUserId(User)!;
        var notifications = await _notificationService.GetRecentAsync(userId, IsStaff, take: int.MaxValue);

        var headers = new[] { "Type", "Title", "Message", "Read", "Date" };
        var rows = notifications.Select(n => new[]
        {
            n.Type.ToString(), n.Title, n.Message.Split('[')[0], n.IsRead ? "Yes" : "No",
            n.CreatedAt.ToLocalTime().ToString("d MMM yyyy HH:mm")
        });
        var excel = _exportService.ExportToExcel("Notifications", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"notifications-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAllAsReadAsync(userId, IsStaff);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
