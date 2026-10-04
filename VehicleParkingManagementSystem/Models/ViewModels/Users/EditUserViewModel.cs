using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models.ViewModels.Users;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Display only.</summary>
    public string? Email { get; set; }

    [Required, StringLength(256), Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required, Display(Name = "Role")]
    public string Role { get; set; } = AppRoles.Driver;

    /// <summary>Leave empty to keep the current password.</summary>
    [DataType(DataType.Password), Display(Name = "New Password")]
    public string? NewPassword { get; set; }
}
