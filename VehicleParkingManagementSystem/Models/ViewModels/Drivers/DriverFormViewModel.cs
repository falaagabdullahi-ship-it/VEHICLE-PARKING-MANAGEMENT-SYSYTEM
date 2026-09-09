using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.Drivers;

public class DriverFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100), Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20), Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(50), Display(Name = "License Number")]
    public string? LicenseNumber { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }
}
