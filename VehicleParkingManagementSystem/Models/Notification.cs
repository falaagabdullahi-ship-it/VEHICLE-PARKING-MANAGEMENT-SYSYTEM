namespace VehicleParkingManagementSystem.Models;

public enum NotificationType
{
    ParkingFull,
    ReservationReminder,
    CheckoutReminder,
    AutoCheckout,
    General
}

public class Notification
{
    public int Id { get; set; }

    /// <summary>Null means the notification is broadcast to all Admins/Officers.</summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public NotificationType Type { get; set; } = NotificationType.General;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
