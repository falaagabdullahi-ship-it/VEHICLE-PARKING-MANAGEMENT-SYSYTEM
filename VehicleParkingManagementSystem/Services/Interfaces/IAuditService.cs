namespace VehicleParkingManagementSystem.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string? userId, string action, string entityName, string? entityId = null, string? details = null, string? ipAddress = null);
}
