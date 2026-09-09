using Microsoft.AspNetCore.Identity;
using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@parking.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
        }

        if (!db.ParkingAreas.Any())
        {
            var groundFloor = new ParkingArea
            {
                Name = "Ground Floor",
                Location = "Main Building - Ground Level",
                Description = "General parking for cars and motorcycles near the main entrance."
            };
            var basement = new ParkingArea
            {
                Name = "Basement A",
                Location = "Main Building - Basement",
                Description = "Covered parking including VIP and disabled-accessible slots."
            };

            db.ParkingAreas.AddRange(groundFloor, basement);
            await db.SaveChangesAsync();
        }

        if (!db.PricingSettings.Any())
        {
            db.PricingSettings.AddRange(
                new PricingSetting { VehicleType = VehicleType.Car, HourlyRate = 100m, MinimumCharge = 50m },
                new PricingSetting { VehicleType = VehicleType.Motorcycle, HourlyRate = 50m, MinimumCharge = 20m },
                new PricingSetting { VehicleType = VehicleType.Van, HourlyRate = 120m, MinimumCharge = 60m },
                new PricingSetting { VehicleType = VehicleType.Truck, HourlyRate = 200m, MinimumCharge = 100m },
                new PricingSetting { VehicleType = VehicleType.Bus, HourlyRate = 200m, MinimumCharge = 100m }
            );

            await db.SaveChangesAsync();
        }

        if (!db.ParkingPackages.Any())
        {
            db.ParkingPackages.AddRange(
                new ParkingPackage { VehicleType = VehicleType.Car, Name = "2 Hour", DurationMinutes = 120, Price = 200m },
                new ParkingPackage { VehicleType = VehicleType.Car, Name = "6 Hour", DurationMinutes = 360, Price = 500m },
                new ParkingPackage { VehicleType = VehicleType.Motorcycle, Name = "2 Hour", DurationMinutes = 120, Price = 100m },
                new ParkingPackage { VehicleType = VehicleType.Motorcycle, Name = "6 Hour", DurationMinutes = 360, Price = 250m },
                new ParkingPackage { VehicleType = VehicleType.Van, Name = "2 Hour", DurationMinutes = 120, Price = 240m },
                new ParkingPackage { VehicleType = VehicleType.Van, Name = "6 Hour", DurationMinutes = 360, Price = 600m },
                new ParkingPackage { VehicleType = VehicleType.Truck, Name = "2 Hour", DurationMinutes = 120, Price = 400m },
                new ParkingPackage { VehicleType = VehicleType.Truck, Name = "6 Hour", DurationMinutes = 360, Price = 1000m },
                new ParkingPackage { VehicleType = VehicleType.Bus, Name = "2 Hour", DurationMinutes = 120, Price = 400m },
                new ParkingPackage { VehicleType = VehicleType.Bus, Name = "6 Hour", DurationMinutes = 360, Price = 1000m }
            );

            await db.SaveChangesAsync();
        }
    }
}
