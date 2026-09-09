using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Dashboard;
using VehicleParkingManagementSystem.Models.ViewModels.ParkingOperations;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

/// <summary>
/// Aggregates data across many entities for dashboards/analytics, so it reads directly
/// via AppDbContext rather than through the per-entity repositories.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardViewModel> GetOperationalDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var activeSessions = await _db.ParkingRecords.CountAsync(r => r.CheckOutTime == null);

        var vehiclesToday = await _db.ParkingRecords.CountAsync(r => r.CheckInTime >= today);
        var vehiclesYesterday = await _db.ParkingRecords.CountAsync(r => r.CheckInTime >= yesterday && r.CheckInTime < today);
        var vehiclesCheckedOutToday = await _db.ParkingRecords.CountAsync(r => r.CheckOutTime >= today);
        var vehiclesCheckedOutYesterday = await _db.ParkingRecords.CountAsync(r => r.CheckOutTime >= yesterday && r.CheckOutTime < today);
        var vehiclesDeletedToday = await _db.AuditLogs.CountAsync(a => a.Action == "Delete" && a.EntityName == "Vehicle" && a.Timestamp >= today);
        var vehiclesDeletedYesterday = await _db.AuditLogs.CountAsync(a => a.Action == "Delete" && a.EntityName == "Vehicle" && a.Timestamp >= yesterday && a.Timestamp < today);
        var vehiclesExitedToday = vehiclesCheckedOutToday + vehiclesDeletedToday;
        var vehiclesExitedYesterday = vehiclesCheckedOutYesterday + vehiclesDeletedYesterday;
        var totalVehicles = await _db.Vehicles.CountAsync();

        var todayRevenue = await _db.ParkingRecords
            .Where(r => r.CheckOutTime >= today)
            .SumAsync(r => (decimal?)r.Fee) ?? 0m;

        var monthRevenue = await _db.ParkingRecords
            .Where(r => r.CheckOutTime >= monthStart)
            .SumAsync(r => (decimal?)r.Fee) ?? 0m;

        var lastMonthRevenue = await _db.ParkingRecords
            .Where(r => r.CheckOutTime >= lastMonthStart && r.CheckOutTime < monthStart)
            .SumAsync(r => (decimal?)r.Fee) ?? 0m;

        var recentCheckIns = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .OrderByDescending(r => r.CheckInTime)
            .Take(5)
            .Select(r => new RecentActivityItem
            {
                RecordId = r.Id,
                Kind = "CheckIn",
                ActivityLabel = "Vehicle Check-In",
                VehicleNumber = r.Vehicle!.VehicleNumber,
                Location = r.ParkingArea != null ? r.ParkingArea.Name : "-",
                Status = "Completed",
                Timestamp = r.CheckInTime,
                Icon = "bi-box-arrow-in-right"
            })
            .ToListAsync();

        var recentCheckOuts = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => r.CheckOutTime != null)
            .OrderByDescending(r => r.CheckOutTime)
            .Take(5)
            .Select(r => new RecentActivityItem
            {
                RecordId = r.Id,
                Kind = "CheckOut",
                ActivityLabel = "Vehicle Check-Out",
                VehicleNumber = r.Vehicle!.VehicleNumber,
                Location = r.ParkingArea != null ? r.ParkingArea.Name : "-",
                Status = "Completed",
                Timestamp = r.CheckOutTime!.Value,
                Icon = "bi-box-arrow-right"
            })
            .ToListAsync();

        var recentReservations = await _db.Reservations
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentActivityItem
            {
                RecordId = r.Id,
                Kind = "Reservation",
                ActivityLabel = "Reservation Created",
                VehicleNumber = r.Vehicle!.VehicleNumber,
                Location = r.ParkingArea != null ? r.ParkingArea.Name : "-",
                Status = "Reserved",
                Timestamp = r.CreatedAt,
                Icon = "bi-calendar-check"
            })
            .ToListAsync();

        var recentActivities = recentCheckIns.Concat(recentCheckOuts).Concat(recentReservations)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();

        return new DashboardViewModel
        {
            ActiveSessions = activeSessions,
            VehiclesToday = vehiclesToday,
            VehiclesExitedToday = vehiclesExitedToday,
            TotalRegisteredVehicles = totalVehicles,
            TodayRevenue = todayRevenue,
            MonthRevenue = monthRevenue,
            VehiclesTodayTrendPct = vehiclesYesterday == 0 ? null : Math.Round((vehiclesToday - vehiclesYesterday) * 100.0 / vehiclesYesterday, 0),
            VehiclesExitedTodayTrendPct = vehiclesExitedYesterday == 0 ? null : Math.Round((vehiclesExitedToday - vehiclesExitedYesterday) * 100.0 / vehiclesExitedYesterday, 0),
            RevenueTrendPct = lastMonthRevenue == 0 ? null : Math.Round((double)((monthRevenue - lastMonthRevenue) * 100m / lastMonthRevenue), 0),
            RecentActivities = recentActivities,
            ParkingMap = await GetParkingMapAsync()
        };
    }

    public async Task<PagedResult<ExitedVehicleItem>> GetExitedActivityAsync(DateTime date, string? search, int pageNumber, int pageSize)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var checkOuts = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => r.CheckOutTime >= dayStart && r.CheckOutTime < dayEnd)
            .Select(r => new ExitedVehicleItem
            {
                ParkingRecordId = r.Id,
                VehicleNumber = r.Vehicle!.VehicleNumber,
                OwnerName = r.Vehicle!.OwnerName,
                AreaName = r.ParkingArea != null ? r.ParkingArea.Name : null,
                CheckInTime = r.CheckInTime,
                EventTime = r.CheckOutTime!.Value,
                Fee = r.Fee,
                Status = "Completed"
            })
            .ToListAsync();

        var deletionLogs = await _db.AuditLogs
            .Where(a => a.Action == "Delete" && a.EntityName == "Vehicle" && a.Timestamp >= dayStart && a.Timestamp < dayEnd)
            .ToListAsync();

        var deletions = deletionLogs.Select(a =>
        {
            var parts = a.Details?.Split(" - ", 2);
            return new ExitedVehicleItem
            {
                ParkingRecordId = null,
                VehicleNumber = parts is { Length: > 0 } ? parts[0] : $"Vehicle #{a.EntityId}",
                OwnerName = parts is { Length: 2 } ? parts[1] : "-",
                AreaName = null,
                CheckInTime = null,
                EventTime = a.Timestamp,
                Fee = null,
                Status = "Deleted"
            };
        });

        IEnumerable<ExitedVehicleItem> merged = checkOuts.Concat(deletions);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            merged = merged.Where(i =>
                i.VehicleNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                i.OwnerName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = merged.OrderByDescending(i => i.EventTime).ToList();

        var items = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<ExitedVehicleItem>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = ordered.Count
        };
    }

    public async Task<List<ParkingMapAreaViewModel>> GetParkingMapAsync()
    {
        var areas = await _db.ParkingAreas.OrderBy(a => a.Name).ToListAsync();

        var activeRecords = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Where(r => r.CheckOutTime == null)
            .ToListAsync();

        var byArea = activeRecords.ToLookup(r => r.ParkingAreaId);

        var map = areas.Select(a => new ParkingMapAreaViewModel
        {
            AreaId = a.Id,
            AreaName = a.Name,
            IsVip = a.IsVip,
            ActiveCount = byArea[a.Id].Count(),
            VehicleNumbers = byArea[a.Id].Select(r => r.Vehicle!.VehicleNumber).ToList()
        }).ToList();

        var unassigned = byArea[null].ToList();
        if (unassigned.Count > 0)
        {
            map.Add(new ParkingMapAreaViewModel
            {
                AreaId = null,
                AreaName = "Unassigned",
                ActiveCount = unassigned.Count,
                VehicleNumbers = unassigned.Select(r => r.Vehicle!.VehicleNumber).ToList()
            });
        }

        return map;
    }

    public async Task<DriverDashboardViewModel> GetDriverDashboardAsync(string applicationUserId)
    {
        var vehicleIds = await _db.Vehicles
            .Where(v => v.Driver!.ApplicationUserId == applicationUserId)
            .Select(v => v.Id)
            .ToListAsync();

        var activeSession = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .Where(r => vehicleIds.Contains(r.VehicleId) && r.CheckOutTime == null)
            .FirstOrDefaultAsync();

        var upcomingReservations = await _db.Reservations
            .CountAsync(r => vehicleIds.Contains(r.VehicleId)
                && r.Status == ReservationStatus.Confirmed
                && r.ReservedTo >= DateTime.UtcNow);

        var totalSpent = await _db.ParkingRecords
            .Where(r => vehicleIds.Contains(r.VehicleId) && r.CheckOutTime != null)
            .SumAsync(r => (decimal?)r.Fee) ?? 0m;

        return new DriverDashboardViewModel
        {
            VehicleCount = vehicleIds.Count,
            HasActiveSession = activeSession is not null,
            ActiveVehicleNumber = activeSession?.Vehicle?.VehicleNumber,
            ActiveAreaName = activeSession?.ParkingArea?.Name,
            ActiveCheckInTime = activeSession?.CheckInTime,
            UpcomingReservationCount = upcomingReservations,
            TotalSpent = totalSpent
        };
    }

    public async Task<ChartDataViewModel> GetDailyVisitorsChartAsync(int days = 7)
    {
        var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var records = await _db.ParkingRecords
            .Where(r => r.CheckInTime >= start)
            .Select(r => r.CheckInTime.Date)
            .ToListAsync();

        var grouped = records.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());

        var labels = new List<string>();
        var data = new List<decimal>();
        for (var day = start; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
        {
            labels.Add(day.ToString("MMM d"));
            data.Add(grouped.TryGetValue(day, out var count) ? count : 0);
        }

        return new ChartDataViewModel { Labels = labels, Data = data };
    }

    public async Task<ChartDataViewModel> GetWeeklyRevenueChartAsync(int days = 7)
    {
        var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var records = await _db.ParkingRecords
            .Where(r => r.CheckOutTime != null && r.CheckOutTime >= start)
            .Select(r => new { CheckOutTime = r.CheckOutTime!.Value, r.Fee })
            .ToListAsync();

        var grouped = records
            .GroupBy(r => r.CheckOutTime.Date)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Fee ?? 0m));

        var labels = new List<string>();
        var data = new List<decimal>();
        for (var day = start; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
        {
            labels.Add(day.ToString("MMM d"));
            data.Add(grouped.TryGetValue(day, out var sum) ? sum : 0);
        }

        return new ChartDataViewModel { Labels = labels, Data = data };
    }

    public async Task<ChartDataViewModel> GetMonthlyRevenueChartAsync(int months = 6)
    {
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var records = await _db.ParkingRecords
            .Where(r => r.CheckOutTime != null && r.CheckOutTime >= start)
            .Select(r => new { CheckOutTime = r.CheckOutTime!.Value, r.Fee })
            .ToListAsync();

        var grouped = records
            .GroupBy(r => new DateTime(r.CheckOutTime.Year, r.CheckOutTime.Month, 1))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Fee ?? 0m));

        var labels = new List<string>();
        var data = new List<decimal>();
        for (var month = start; month <= DateTime.UtcNow; month = month.AddMonths(1))
        {
            var key = new DateTime(month.Year, month.Month, 1);
            labels.Add(key.ToString("MMM yyyy"));
            data.Add(grouped.TryGetValue(key, out var sum) ? sum : 0);
        }

        return new ChartDataViewModel { Labels = labels, Data = data };
    }

    public async Task<ChartDataViewModel> GetVehicleTypeChartAsync()
    {
        var counts = await _db.Vehicles
            .GroupBy(v => v.VehicleType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return new ChartDataViewModel
        {
            Labels = counts.Select(c => c.Type.ToString()).ToList(),
            Data = counts.Select(c => (decimal)c.Count).ToList()
        };
    }

    public async Task<ChartDataViewModel> GetPeakHoursChartAsync()
    {
        var hours = await _db.ParkingRecords
            .Select(r => r.CheckInTime.Hour)
            .ToListAsync();

        var grouped = hours.GroupBy(h => h).ToDictionary(g => g.Key, g => g.Count());

        var labels = new List<string>();
        var data = new List<decimal>();
        for (var hour = 0; hour < 24; hour++)
        {
            labels.Add($"{hour:D2}:00");
            data.Add(grouped.TryGetValue(hour, out var count) ? count : 0);
        }

        return new ChartDataViewModel { Labels = labels, Data = data };
    }
}
