using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Services;

namespace VehicleParkingManagementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AuditLogsController : Controller
{
    private const int PageSize = 25;

    private readonly AppDbContext _db;
    private readonly ReportExportService _exportService;

    public AuditLogsController(AppDbContext db, ReportExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string? action, string? entityName, int page = 1)
    {
        var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        var totalCount = await query.CountAsync();
        page = page < 1 ? 1 : page;

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Action = action;
        ViewBag.EntityName = entityName;
        ViewBag.Actions = await _db.AuditLogs.Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();
        ViewBag.Entities = await _db.AuditLogs.Select(a => a.EntityName).Distinct().OrderBy(a => a).ToListAsync();

        return View(new PagedResult<AuditLog>
        {
            Items = items,
            PageNumber = page,
            PageSize = PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<IActionResult> ExportExcel(string? action, string? entityName)
    {
        var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
        if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(a => a.EntityName == entityName);

        var logs = await query.OrderByDescending(a => a.Timestamp).ToListAsync();

        var headers = new[] { "Timestamp", "User", "Action", "Entity", "Details", "IP" };
        var rows = logs.Select(l => new[]
        {
            l.Timestamp.ToLocalTime().ToString("d MMM yyyy HH:mm:ss"), l.User?.Email ?? "System", l.Action,
            l.EntityName + (l.EntityId is not null ? $" #{l.EntityId}" : ""), l.Details ?? "-", l.IpAddress ?? "-"
        });
        var excel = _exportService.ExportToExcel("Audit Logs", headers, rows);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"audit-logs-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }
}
