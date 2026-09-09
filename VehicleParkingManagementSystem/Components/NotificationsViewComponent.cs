using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Models.ViewModels.Notifications;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Components;

public class NotificationsViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsViewComponent(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        if (userId is null)
        {
            return View(new NotificationsBellViewModel());
        }

        var isStaff = UserClaimsPrincipal.IsInRole(AppRoles.Admin) || UserClaimsPrincipal.IsInRole(AppRoles.ParkingOfficer);

        if (isStaff)
        {
            await _notificationService.GenerateSystemNotificationsAsync();
        }

        var model = new NotificationsBellViewModel
        {
            UnreadCount = await _notificationService.GetUnreadCountAsync(userId, isStaff),
            Recent = await _notificationService.GetRecentAsync(userId, isStaff)
        };

        return View(model);
    }
}
