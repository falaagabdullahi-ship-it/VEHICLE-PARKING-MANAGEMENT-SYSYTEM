using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Data;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task GenerateSystemNotificationsAsync()
    {
        var now = DateTime.UtcNow;
        var created = false;

        // Reservation Reminder: confirmed reservations starting within the next 30 minutes.
        var upcoming = await _db.Reservations
            .Include(r => r.Vehicle)
            .Where(r => r.Status == ReservationStatus.Confirmed
                && r.ReservedFrom >= now && r.ReservedFrom <= now.AddMinutes(30))
            .ToListAsync();

        foreach (var reservation in upcoming)
        {
            var marker = $"[Reservation:{reservation.Id}]";
            var alreadyNotified = await _db.Notifications.AnyAsync(n =>
                n.Type == NotificationType.ReservationReminder && n.Message.Contains(marker));

            if (!alreadyNotified)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = reservation.CreatedByUserId,
                    Type = NotificationType.ReservationReminder,
                    Title = "Reservation Starting Soon",
                    Message = $"Your reservation for {reservation.Vehicle!.VehicleNumber} starts at {reservation.ReservedFrom.ToLocalTime():d MMM, HH:mm}. {marker}",
                    CreatedAt = now
                });
                created = true;
            }
        }

        // Checkout Reminder: vehicles parked for more than 12 hours without checking out.
        var overstayed = await _db.ParkingRecords
            .Include(r => r.Vehicle)
            .Where(r => r.CheckOutTime == null && r.CheckInTime <= now.AddHours(-12))
            .ToListAsync();

        foreach (var record in overstayed)
        {
            var marker = $"[ParkingRecord:{record.Id}]";
            var alreadyNotified = await _db.Notifications.AnyAsync(n =>
                n.Type == NotificationType.CheckoutReminder && n.Message.Contains(marker));

            if (!alreadyNotified)
            {
                _db.Notifications.Add(new Notification
                {
                    Type = NotificationType.CheckoutReminder,
                    Title = "Overstay Alert",
                    Message = $"{record.Vehicle!.VehicleNumber} has been parked for over 12 hours and has not checked out. {marker}",
                    CreatedAt = now
                });
                created = true;
            }
        }

        if (created)
        {
            await _db.SaveChangesAsync();
        }
    }

    public async Task NotifyAsync(NotificationType type, string title, string message, string? userId = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetRecentAsync(string userId, bool isStaff, int take = 8)
    {
        var query = _db.Notifications.AsQueryable();

        query = isStaff
            ? query.Where(n => n.UserId == null || n.UserId == userId)
            : query.Where(n => n.UserId == userId);

        return await query.OrderByDescending(n => n.CreatedAt).Take(take).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId, bool isStaff)
    {
        var query = _db.Notifications.Where(n => !n.IsRead);

        query = isStaff
            ? query.Where(n => n.UserId == null || n.UserId == userId)
            : query.Where(n => n.UserId == userId);

        return await query.CountAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification is null) return;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId, bool isStaff)
    {
        var query = _db.Notifications.Where(n => !n.IsRead);

        query = isStaff
            ? query.Where(n => n.UserId == null || n.UserId == userId)
            : query.Where(n => n.UserId == userId);

        var items = await query.ToListAsync();
        foreach (var item in items) item.IsRead = true;

        await _db.SaveChangesAsync();
    }
}
