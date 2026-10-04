namespace VehicleParkingManagementSystem.Models.ViewModels.Settings;

public class SettingsViewModel
{
    public List<PricingSettingRow> PricingRows { get; set; } = new();
    public List<ParkingPackageRow> Packages { get; set; } = new();
    public NewParkingPackageViewModel NewPackage { get; set; } = new();
    public List<ParkingArea> ParkingAreas { get; set; } = new();
}
