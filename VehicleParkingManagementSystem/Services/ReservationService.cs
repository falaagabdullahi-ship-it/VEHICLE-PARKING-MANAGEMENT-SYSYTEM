using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IVehicleRepository _vehicles;

    public ReservationService(
        IReservationRepository reservations,
        IVehicleRepository vehicles)
    {
        _reservations = reservations;
        _vehicles = vehicles;
    }

    public async Task<ServiceResult<Reservation>> CreateAsync(int vehicleId, int? parkingAreaId, DateTime reservedFrom, DateTime reservedTo, string createdByUserId)
    {
        if (reservedFrom >= reservedTo)
        {
            return ServiceResult<Reservation>.Failure("The reservation end time must be after the start time.");
        }

        if (reservedTo <= DateTime.UtcNow)
        {
            return ServiceResult<Reservation>.Failure("You cannot reserve a slot for a time in the past.");
        }

        var vehicle = await _vehicles.GetByIdAsync(vehicleId);
        if (vehicle is null)
        {
            return ServiceResult<Reservation>.Failure("Vehicle not found.");
        }

        await ExpireOverdueReservationsAsync();

        var reservation = new Reservation
        {
            VehicleId = vehicleId,
            ParkingAreaId = parkingAreaId,
            ReservedFrom = reservedFrom,
            ReservedTo = reservedTo,
            Status = ReservationStatus.Confirmed,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _reservations.AddAsync(reservation);
        await _reservations.SaveChangesAsync();

        return ServiceResult<Reservation>.Success(reservation);
    }

    public async Task<ServiceResult> CancelAsync(int reservationId, string? cancellationReason)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId);
        if (reservation is null)
        {
            return ServiceResult.Failure("Reservation not found.");
        }

        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
        {
            return ServiceResult.Failure("Only pending or confirmed reservations can be cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;
        reservation.CancellationReason = cancellationReason;

        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<PagedResult<Reservation>> GetPagedAsync(string? search, string? scopedToUserId, int pageNumber, int pageSize)
    {
        await ExpireOverdueReservationsAsync();
        return await _reservations.GetPagedAsync(search, scopedToUserId, pageNumber, pageSize);
    }

    public Task<Reservation?> GetDetailsAsync(int id) => _reservations.GetWithDetailsAsync(id);

    private async Task ExpireOverdueReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var overdue = await _reservations.Query()
            .Where(r => r.Status == ReservationStatus.Confirmed && r.ReservedTo < now)
            .ToListAsync();

        if (overdue.Count == 0) return;

        foreach (var reservation in overdue)
        {
            reservation.Status = ReservationStatus.Expired;
            _reservations.Update(reservation);
        }

        await _reservations.SaveChangesAsync();
    }
}
