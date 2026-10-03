using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels;
using VehicleParkingManagementSystem.Models.ViewModels.Vehicles;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicles;
    private readonly IDriverRepository _drivers;
    private readonly IParkingRecordRepository _parkingRecords;
    private readonly IReservationRepository _reservations;

    public VehicleService(
        IVehicleRepository vehicles,
        IDriverRepository drivers,
        IParkingRecordRepository parkingRecords,
        IReservationRepository reservations)
    {
        _vehicles = vehicles;
        _drivers = drivers;
        _parkingRecords = parkingRecords;
        _reservations = reservations;
    }

    public async Task<PagedResult<Vehicle>> GetPagedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _vehicles.Query().Include(v => v.Driver).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(v =>
                v.VehicleNumber.Contains(term) ||
                v.OwnerName.Contains(term) ||
                v.VehicleModel.Contains(term) ||
                v.PhoneNumber.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(v => v.RegistrationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Vehicle>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<Vehicle?> GetByIdAsync(int id) => _vehicles.GetWithDetailsAsync(id);

    public async Task<ServiceResult> CreateAsync(VehicleFormViewModel model)
    {
        if (!await _drivers.ExistsAsync(d => d.Id == model.DriverId))
        {
            return ServiceResult.Failure("The selected driver does not exist.");
        }

        if (await _vehicles.ExistsAsync(v => v.VehicleNumber == model.VehicleNumber))
        {
            return ServiceResult.Failure("A vehicle with this number is already registered.");
        }

        var vehicle = new Vehicle
        {
            VehicleNumber = model.VehicleNumber.Trim().ToUpperInvariant(),
            OwnerName = model.OwnerName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            VehicleType = model.VehicleType,
            VehicleModel = model.VehicleModel.Trim(),
            Color = model.Color.Trim(),
            RegistrationDate = model.RegistrationDate,
            DriverId = model.DriverId
        };

        await _vehicles.AddAsync(vehicle);
        await _vehicles.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(VehicleFormViewModel model)
    {
        var vehicle = await _vehicles.GetByIdAsync(model.Id);
        if (vehicle is null)
        {
            return ServiceResult.Failure("Vehicle not found.");
        }

        if (!await _drivers.ExistsAsync(d => d.Id == model.DriverId))
        {
            return ServiceResult.Failure("The selected driver does not exist.");
        }

        var normalizedNumber = model.VehicleNumber.Trim().ToUpperInvariant();
        if (await _vehicles.ExistsAsync(v => v.VehicleNumber == normalizedNumber && v.Id != model.Id))
        {
            return ServiceResult.Failure("A vehicle with this number is already registered.");
        }

        vehicle.VehicleNumber = normalizedNumber;
        vehicle.OwnerName = model.OwnerName.Trim();
        vehicle.PhoneNumber = model.PhoneNumber.Trim();
        vehicle.VehicleType = model.VehicleType;
        vehicle.VehicleModel = model.VehicleModel.Trim();
        vehicle.Color = model.Color.Trim();
        vehicle.RegistrationDate = model.RegistrationDate;
        vehicle.DriverId = model.DriverId;

        _vehicles.Update(vehicle);
        await _vehicles.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var vehicle = await _vehicles.GetByIdAsync(id);
        if (vehicle is null)
        {
            return ServiceResult.Failure("Vehicle not found.");
        }

        try
        {
            // Remove the vehicle's parking and reservation history so the delete is not blocked.
            var parkingRecords = await _parkingRecords.Query().Where(r => r.VehicleId == id).ToListAsync();
            foreach (var record in parkingRecords)
            {
                _parkingRecords.Remove(record);
            }

            var reservations = await _reservations.Query().Where(r => r.VehicleId == id).ToListAsync();
            foreach (var reservation in reservations)
            {
                _reservations.Remove(reservation);
            }

            _vehicles.Remove(vehicle);
            await _vehicles.SaveChangesAsync();
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("The vehicle could not be deleted. Please try again.");
        }
    }
}
