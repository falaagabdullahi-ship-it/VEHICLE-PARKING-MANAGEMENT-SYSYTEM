using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Reports;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

/// <summary>Reads directly via AppDbContext since reports project across many entities.</summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ParkingActivityReportRow>> GetParkingActivityAsync(DateTime? from, DateTime? to, int? parkingAreaId, string? vehicleNumber)
    {
        var query = _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .AsQueryable();

        if (from.HasValue) query = query.Where(r => r.CheckInTime >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CheckInTime <= to.Value);
        if (parkingAreaId.HasValue) query = query.Where(r => r.ParkingAreaId == parkingAreaId.Value);
        if (!string.IsNullOrWhiteSpace(vehicleNumber)) query = query.Where(r => r.Vehicle!.VehicleNumber.Contains(vehicleNumber));

        return await query
            .OrderByDescending(r => r.CheckInTime)
            .Select(r => new ParkingActivityReportRow
            {
                VehicleNumber = r.Vehicle!.VehicleNumber,
                OwnerName = r.Vehicle.OwnerName,
                ParkingArea = r.ParkingArea != null ? r.ParkingArea.Name : "-",
                CheckInTime = r.CheckInTime,
                CheckOutTime = r.CheckOutTime,
                Fee = r.Fee
            })
            .ToListAsync();
    }

    public async Task<List<ReservationReportRow>> GetReservationsAsync(DateTime? from, DateTime? to, ReservationStatus? status)
    {
        var query = _db.Reservations
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingArea)
            .AsQueryable();

        if (from.HasValue) query = query.Where(r => r.ReservedFrom >= from.Value);
        if (to.HasValue) query = query.Where(r => r.ReservedFrom <= to.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.ReservedFrom)
            .Select(r => new ReservationReportRow
            {
                VehicleNumber = r.Vehicle!.VehicleNumber,
                ParkingArea = r.ParkingArea != null ? r.ParkingArea.Name : "-",
                ReservedFrom = r.ReservedFrom,
                ReservedTo = r.ReservedTo,
                Status = r.Status
            })
            .ToListAsync();
    }

    public async Task<List<VehicleReportRow>> GetVehiclesAsync(int? driverId)
    {
        var query = _db.Vehicles.Include(v => v.Driver).AsQueryable();

        if (driverId.HasValue) query = query.Where(v => v.DriverId == driverId.Value);

        return await query
            .OrderBy(v => v.VehicleNumber)
            .Select(v => new VehicleReportRow
            {
                VehicleNumber = v.VehicleNumber,
                OwnerName = v.OwnerName,
                DriverName = v.Driver!.FirstName + " " + v.Driver.LastName,
                VehicleType = v.VehicleType,
                VehicleModel = v.VehicleModel,
                RegistrationDate = v.RegistrationDate
            })
            .ToListAsync();
    }
}
