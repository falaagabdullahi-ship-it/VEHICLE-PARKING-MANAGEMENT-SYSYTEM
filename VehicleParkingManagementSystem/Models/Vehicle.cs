using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string OwnerName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public VehicleType VehicleType { get; set; }

    [Required, StringLength(100)]
    public string VehicleModel { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Color { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public int DriverId { get; set; }
    public Driver? Driver { get; set; }

    public ICollection<ParkingRecord> ParkingRecords { get; set; } = new List<ParkingRecord>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
