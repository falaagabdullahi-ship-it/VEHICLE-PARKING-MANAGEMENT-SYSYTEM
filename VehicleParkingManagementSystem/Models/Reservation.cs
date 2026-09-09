namespace VehicleParkingManagementSystem.Models;

public class Reservation
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int? ParkingAreaId { get; set; }
    public ParkingArea? ParkingArea { get; set; }

    public DateTime ReservedFrom { get; set; }
    public DateTime ReservedTo { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public ParkingRecord? ParkingRecord { get; set; }
}
