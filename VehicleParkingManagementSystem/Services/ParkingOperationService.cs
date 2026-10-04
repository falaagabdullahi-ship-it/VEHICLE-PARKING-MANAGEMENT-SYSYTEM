using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class ParkingOperationService : IParkingOperationService
{
    private readonly IParkingRecordRepository _records;
    private readonly IVehicleRepository _vehicles;
    private readonly IReservationRepository _reservations;
    private readonly IFeeCalculationService _feeCalculator;
    private readonly IRepository<ParkingPackage> _packages;
    private readonly INotificationService _notifications;

    public ParkingOperationService(
        IParkingRecordRepository records,
        IVehicleRepository vehicles,
        IReservationRepository reservations,
        IFeeCalculationService feeCalculator,
        IRepository<ParkingPackage> packages,
        INotificationService notifications)
    {
        _records = records;
        _vehicles = vehicles;
        _reservations = reservations;
        _feeCalculator = feeCalculator;
        _packages = packages;
        _notifications = notifications;
    }

    public async Task<ServiceResult<ParkingRecord>> CheckInAsync(string vehicleNumber, int? parkingAreaId, int? parkingPackageId, string officerUserId)
    {
        var normalizedNumber = vehicleNumber.Trim().ToUpperInvariant();
        var vehicle = await _vehicles.GetByVehicleNumberAsync(normalizedNumber);
        if (vehicle is null)
        {
            return ServiceResult<ParkingRecord>.Failure("No vehicle is registered with this number. Please register it first.");
        }

        var alreadyParked = await _records.ExistsAsync(r => r.VehicleId == vehicle.Id && r.CheckOutTime == null);
        if (alreadyParked)
        {
            return ServiceResult<ParkingRecord>.Failure("This vehicle is already checked in.");
        }

        var now = DateTime.UtcNow;

        // Fulfill an active reservation for this vehicle first, if one exists.
        var reservation = await _reservations.Query()
            .Where(r => r.VehicleId == vehicle.Id
                && r.Status == ReservationStatus.Confirmed
                && r.ReservedFrom <= now && r.ReservedTo >= now)
            .FirstOrDefaultAsync();

        ParkingPackage? package = null;
        DateTime? paidUntil;
        decimal? fee = null;

        if (reservation is not null)
        {
            // Reservations already define an end time; that time governs auto checkout and the fee is
            // computed at checkout like a normal metered session.
            paidUntil = reservation.ReservedTo;
        }
        else
        {
            if (parkingPackageId is null)
            {
                return ServiceResult<ParkingRecord>.Failure("Please select a parking package to continue.");
            }

            package = await _packages.Query().FirstOrDefaultAsync(p => p.Id == parkingPackageId && p.IsActive);
            if (package is null)
            {
                return ServiceResult<ParkingRecord>.Failure("Selected parking package is not available.");
            }

            if (package.VehicleType != vehicle.VehicleType)
            {
                return ServiceResult<ParkingRecord>.Failure($"Selected package is not valid for a {vehicle.VehicleType}.");
            }

            if (package.ParkingAreaId is not null)
            {
                if (parkingAreaId is not null && parkingAreaId != package.ParkingAreaId)
                {
                    return ServiceResult<ParkingRecord>.Failure("Selected package is not valid for this parking area.");
                }

                // An area-specific package decides where the vehicle parks.
                parkingAreaId = package.ParkingAreaId;
            }

            paidUntil = now.AddMinutes(package.DurationMinutes);
            fee = package.Price;
        }

        var record = new ParkingRecord
        {
            VehicleId = vehicle.Id,
            ParkingAreaId = reservation?.ParkingAreaId ?? parkingAreaId,
            CheckInTime = now,
            CheckedInByUserId = officerUserId,
            ReservationId = reservation?.Id,
            ParkingPackageId = package?.Id,
            PaidUntil = paidUntil,
            Fee = fee
        };

        if (reservation is not null)
        {
            reservation.Status = ReservationStatus.Completed;
            _reservations.Update(reservation);
        }

        await _records.AddAsync(record);
        await _records.SaveChangesAsync();

        return ServiceResult<ParkingRecord>.Success(record);
    }

    public async Task<ServiceResult<ParkingRecord>> CheckOutAsync(int parkingRecordId, string officerUserId)
    {
        var record = await _records.GetWithDetailsAsync(parkingRecordId);
        if (record is null)
        {
            return ServiceResult<ParkingRecord>.Failure("Parking session not found.");
        }

        if (record.CheckOutTime is not null)
        {
            return ServiceResult<ParkingRecord>.Failure("This session has already been checked out.");
        }

        var checkOutTime = DateTime.UtcNow;

        // Prepaid sessions keep the fee they already paid at check-in, even on early exit (no proration/refund).
        var fee = record.ParkingPackageId is not null
            ? record.Fee ?? 0m
            : await _feeCalculator.CalculateFeeAsync(record.Vehicle!.VehicleType, checkOutTime - record.CheckInTime);

        record.CheckOutTime = checkOutTime;
        record.Fee = fee;
        record.CheckedOutByUserId = officerUserId;

        _records.Update(record);
        await _records.SaveChangesAsync();

        return ServiceResult<ParkingRecord>.Success(record);
    }

    public async Task<ServiceResult<ParkingRecord>> CreateManualTransactionAsync(string vehicleNumber, int? parkingAreaId, DateTime checkInTime, DateTime? checkOutTime, decimal? fee, string officerUserId)
    {
        var normalizedNumber = vehicleNumber.Trim().ToUpperInvariant();
        var vehicle = await _vehicles.GetByVehicleNumberAsync(normalizedNumber);
        if (vehicle is null)
        {
            return ServiceResult<ParkingRecord>.Failure("No vehicle is registered with this number. Please register it first.");
        }

        if (checkOutTime.HasValue && checkOutTime.Value <= checkInTime)
        {
            return ServiceResult<ParkingRecord>.Failure("The check-out time must be after the check-in time.");
        }

        if (!checkOutTime.HasValue)
        {
            var alreadyParked = await _records.ExistsAsync(r => r.VehicleId == vehicle.Id && r.CheckOutTime == null);
            if (alreadyParked)
            {
                return ServiceResult<ParkingRecord>.Failure("This vehicle is already checked in.");
            }
        }

        var record = new ParkingRecord
        {
            VehicleId = vehicle.Id,
            ParkingAreaId = parkingAreaId,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            CheckedInByUserId = officerUserId,
            CheckedOutByUserId = checkOutTime.HasValue ? officerUserId : null
        };

        if (checkOutTime.HasValue)
        {
            record.Fee = fee ?? await _feeCalculator.CalculateFeeAsync(vehicle.VehicleType, checkOutTime.Value - checkInTime);
        }

        await _records.AddAsync(record);
        await _records.SaveChangesAsync();

        return ServiceResult<ParkingRecord>.Success(record);
    }

    public Task<PagedResult<ParkingRecord>> GetActiveSessionsAsync(string? search, int pageNumber, int pageSize) =>
        _records.GetActiveSessionsPagedAsync(search, pageNumber, pageSize);

    public Task<PagedResult<ParkingRecord>> GetByDateAsync(DateTime date, string? search, int pageNumber, int pageSize) =>
        _records.GetByDatePagedAsync(date, search, pageNumber, pageSize);

    public Task<ParkingRecord?> GetDetailsAsync(int id) => _records.GetWithDetailsAsync(id);

    public async Task<decimal> PreviewFeeAsync(int parkingRecordId)
    {
        var record = await _records.GetWithDetailsAsync(parkingRecordId);
        if (record is null || record.Vehicle is null) return 0m;

        if (record.ParkingPackageId is not null)
        {
            return record.Fee ?? 0m;
        }

        var duration = DateTime.UtcNow - record.CheckInTime;
        return await _feeCalculator.CalculateFeeAsync(record.Vehicle.VehicleType, duration);
    }

    public async Task ProcessExpiredPaidSessionsAsync()
    {
        var now = DateTime.UtcNow;

        var expired = await _records.Query()
            .Include(r => r.Vehicle)
            .Where(r => r.CheckOutTime == null && r.PaidUntil != null && r.PaidUntil <= now)
            .ToListAsync();

        if (expired.Count == 0) return;

        foreach (var record in expired)
        {
            var checkOutTime = record.PaidUntil!.Value;

            if (record.ParkingPackageId is null)
            {
                // Reservation-backed session: fee is metered like a normal checkout, at the reserved end time.
                var duration = checkOutTime - record.CheckInTime;
                record.Fee = await _feeCalculator.CalculateFeeAsync(record.Vehicle!.VehicleType, duration);
            }
            // Prepaid package sessions already have Fee set at check-in; leave it untouched.

            record.CheckOutTime = checkOutTime;
            record.AutoCheckedOut = true;

            _records.Update(record);

            await _notifications.NotifyAsync(
                NotificationType.AutoCheckout,
                "Vehicle Auto Checked-Out",
                $"{record.Vehicle!.VehicleNumber} reached its paid/reserved time and was checked out automatically.");
        }

        await _records.SaveChangesAsync();
    }
}
