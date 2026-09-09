namespace VehicleParkingManagementSystem.Models;

public class ParkingRecord
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int? ParkingAreaId { get; set; }
    public ParkingArea? ParkingArea { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutTime { get; set; }

    public decimal? Fee { get; set; }

    public string CheckedInByUserId { get; set; } = string.Empty;
    public ApplicationUser? CheckedInByUser { get; set; }

    public string? CheckedOutByUserId { get; set; }
    public ApplicationUser? CheckedOutByUser { get; set; }

    /// <summary>Set when this check-in fulfills an existing reservation.</summary>
    public int? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    /// <summary>Set when this check-in was paid for via a prepaid package rather than metered at checkout.</summary>
    public int? ParkingPackageId { get; set; }
    public ParkingPackage? ParkingPackage { get; set; }

    /// <summary>Moment this session's paid/reserved time runs out. Drives automatic checkout.</summary>
    public DateTime? PaidUntil { get; set; }

    public bool AutoCheckedOut { get; set; }

    public bool IsActive => CheckOutTime is null;

    public TimeSpan? Duration => CheckOutTime.HasValue ? CheckOutTime.Value - CheckInTime : null;
}
