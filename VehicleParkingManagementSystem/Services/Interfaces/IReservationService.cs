using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IReservationService
{
    Task<ServiceResult<Reservation>> CreateAsync(int vehicleId, int? parkingAreaId, DateTime reservedFrom, DateTime reservedTo, string createdByUserId);
    Task<ServiceResult> CancelAsync(int reservationId, string? cancellationReason);
    Task<ServiceResult> DeleteAsync(int reservationId);
    Task<PagedResult<Reservation>> GetPagedAsync(string? search, string? scopedToUserId, int pageNumber, int pageSize);
    Task<Reservation?> GetDetailsAsync(int id);
}
