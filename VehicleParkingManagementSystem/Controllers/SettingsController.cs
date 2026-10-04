using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Settings;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingsController(AppDbContext db, IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new SettingsViewModel
        {
            PricingRows = await _db.PricingSettings
                .OrderBy(p => p.VehicleType)
                .Select(p => new PricingSettingRow { Id = p.Id, VehicleType = p.VehicleType, HourlyRate = p.HourlyRate, MinimumCharge = p.MinimumCharge })
                .ToListAsync(),
            Packages = await _db.ParkingPackages
                .OrderBy(p => p.VehicleType).ThenBy(p => p.DurationMinutes)
                .Select(p => new ParkingPackageRow
                {
                    Id = p.Id,
                    VehicleType = p.VehicleType,
                    Name = p.Name,
                    ParkingAreaName = p.ParkingArea != null ? p.ParkingArea.Name : null,
                    DurationHours = p.DurationMinutes / 60m,
                    Price = p.Price,
                    IsActive = p.IsActive
                })
                .ToListAsync(),
            ParkingAreas = await _db.ParkingAreas.OrderBy(a => a.Name).ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePricing(List<PricingSettingRow> rows)
    {
        foreach (var row in rows)
        {
            if (row.HourlyRate < 0 || row.MinimumCharge < 0) continue;

            var setting = await _db.PricingSettings.FindAsync(row.Id);
            if (setting is null) continue;

            setting.HourlyRate = row.HourlyRate;
            setting.MinimumCharge = row.MinimumCharge;
        }

        await _db.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Update", "PricingSettings");

        TempData["StatusMessage"] = "Pricing settings updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePackages(List<ParkingPackageRow> rows)
    {
        foreach (var row in rows)
        {
            if (row.DurationHours <= 0 || row.Price < 0) continue;

            var package = await _db.ParkingPackages.FindAsync(row.Id);
            if (package is null) continue;

            package.DurationMinutes = (int)Math.Round(row.DurationHours * 60);
            package.Price = row.Price;
            package.IsActive = row.IsActive;
        }

        await _db.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Update", "ParkingPackages");

        TempData["StatusMessage"] = "Parking packages updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePackage(NewParkingPackageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Please provide a valid name, duration and price for the new package.";
            return RedirectToAction(nameof(Index));
        }

        if (model.ParkingAreaId is not null && !await _db.ParkingAreas.AnyAsync(a => a.Id == model.ParkingAreaId))
        {
            TempData["StatusMessage"] = "The selected parking area no longer exists.";
            return RedirectToAction(nameof(Index));
        }

        _db.ParkingPackages.Add(new ParkingPackage
        {
            VehicleType = model.VehicleType,
            Name = model.Name.Trim(),
            ParkingAreaId = model.ParkingAreaId,
            DurationMinutes = (int)Math.Round(model.DurationHours * 60),
            Price = model.Price,
            IsActive = true
        });

        await _db.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Create", "ParkingPackages", details: model.Name);

        TempData["StatusMessage"] = "Parking package added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var package = await _db.ParkingPackages.FindAsync(id);
        if (package is not null)
        {
            _db.ParkingPackages.Remove(package);
            try
            {
                await _db.SaveChangesAsync();
                await _auditService.LogAsync(_userManager.GetUserId(User), "Delete", "ParkingPackages", entityId: id.ToString(), details: package.Name);
                TempData["StatusMessage"] = "Parking package removed.";
            }
            catch (DbUpdateException)
            {
                TempData["StatusMessage"] = "This package has already been used for check-ins and can't be deleted. Mark it inactive instead.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
