using System.ComponentModel.DataAnnotations;

namespace VehicleParkingManagementSystem.Models;

public class ParkingArea
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsVip { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
