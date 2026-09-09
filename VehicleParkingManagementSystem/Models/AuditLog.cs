namespace VehicleParkingManagementSystem.Models;

public class AuditLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>e.g. Create, Update, Delete, Login, Logout, CheckIn, CheckOut.</summary>
    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
