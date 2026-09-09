using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Users;
using VehicleParkingManagementSystem.Services;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UsersController : Controller
{
    private const int PageSize = 15;

    private readonly IUserManagementService _userManagementService;
    private readonly IAuditService _auditService;
    private readonly ReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(
        IUserManagementService userManagementService,
        IAuditService auditService,
        ReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _userManagementService = userManagementService;
        _auditService = auditService;
        _exportService = exportService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _userManagementService.GetPagedAsync(search, page < 1 ? 1 : page, PageSize);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> ExportExcel(string? search)
    {
        var result = await _userManagementService.GetPagedAsync(search, 1, int.MaxValue);
        var headers = new[] { "Name", "Email", "Phone", "Role(s)", "Status" };
        var rows = result.Items.Select(u => new[]
        {
            u.FullName, u.Email, u.PhoneNumber, string.Join(", ", u.Roles), u.IsLockedOut ? "Locked" : "Active"
        });
        var excel = _exportService.ExportToExcel("Users", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"users-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public IActionResult Create() => View(new CreateStaffUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _userManagementService.CreateStaffUserAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        await _auditService.LogAsync(_userManager.GetUserId(User), "Create", "User", details: $"{model.Email} ({model.Role})");
        TempData["StatusMessage"] = $"{model.Role} account created for {model.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var result = await _userManagementService.ToggleLockoutAsync(id, currentUserId);

        if (result.Succeeded)
        {
            await _auditService.LogAsync(currentUserId, "ToggleLockout", "User", id);
        }

        TempData["StatusMessage"] = result.Succeeded ? "Account status updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
