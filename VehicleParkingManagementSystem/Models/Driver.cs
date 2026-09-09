using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models;

public class Driver
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;

    /// <summary>Set when this driver profile is linked to a self-service login account.</summary>
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
