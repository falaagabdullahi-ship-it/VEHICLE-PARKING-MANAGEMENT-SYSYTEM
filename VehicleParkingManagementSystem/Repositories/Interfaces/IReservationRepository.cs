using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;

namespace VehicleParkingManagementSystem.Repositories.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<Reservation?> GetWithDetailsAsync(int id);
    Task<PagedResult<Reservation>> GetPagedAsync(string? search, string? userId, int pageNumber, int pageSize);
}
