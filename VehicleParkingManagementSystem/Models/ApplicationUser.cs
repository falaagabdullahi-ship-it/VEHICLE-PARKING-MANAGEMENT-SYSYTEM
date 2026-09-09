using Microsoft.AspNetCore.Identity;

namespace VehicleParkingManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Web-relative path (e.g. "/uploads/profiles/{file}") to the user's profile picture, or null if unset.</summary>
    public string? ProfilePicturePath { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
