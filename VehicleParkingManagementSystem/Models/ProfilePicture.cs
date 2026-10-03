namespace VehicleParkingManagementSystem.Models;

/// <summary>
/// A user's profile picture stored in the database, so it survives redeploys on hosts with an ephemeral disk.
/// Kept out of <see cref="ApplicationUser"/> so loading the user does not pull the image bytes.
/// </summary>
public class ProfilePicture
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
