using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Models.ViewModels.Notifications;

public class NotificationsBellViewModel
{
    public int UnreadCount { get; set; }
    public List<Notification> Recent { get; set; } = [];
}
