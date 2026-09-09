namespace VehicleParkingManagementSystem.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string ParkingOfficer = "ParkingOfficer";
    public const string Driver = "Driver";

    public static readonly string[] All = [Admin, ParkingOfficer, Driver];
}
