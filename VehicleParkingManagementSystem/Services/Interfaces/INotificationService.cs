using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface INotificationService
{
    /// <summary>Scans current system state and creates Parking Full / Reservation Reminder / Checkout Reminder
    /// notifications that don't already exist. Safe to call frequently (idempotent).</summary>
    Task GenerateSystemNotificationsAsync();

    /// <summary>Inserts a single notification immediately (e.g. for one-off system events like an auto checkout).</summary>
    Task NotifyAsync(NotificationType type, string title, string message, string? userId = null);

    Task<List<Notification>> GetRecentAsync(string userId, bool isStaff, int take = 8);
    Task<int> GetUnreadCountAsync(string userId, bool isStaff);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId, bool isStaff);
}
