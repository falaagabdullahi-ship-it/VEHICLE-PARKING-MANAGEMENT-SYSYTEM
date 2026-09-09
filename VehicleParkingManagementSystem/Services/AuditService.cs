using VehicleParkingManagementSystem.Models;
using VehicleParkingManagementSystem.Repositories.Interfaces;
using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditLogs;

    public AuditService(IRepository<AuditLog> auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task LogAsync(string? userId, string action, string entityName, string? entityId = null, string? details = null, string? ipAddress = null)
    {
        await _auditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });

        await _auditLogs.SaveChangesAsync();
    }
}
