using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

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

        // Standard packages per vehicle type. Any that are missing (matched by type + name) are added
        // on startup, so existing databases get them too without duplicating packages already there.
        var standardPackages = new List<ParkingPackage>();
        AddStandardPackages(standardPackages, VehicleType.Motorcycle, 10m, 18m, 25m, 45m, 80m, 120m);
        AddStandardPackages(standardPackages, VehicleType.Car, 20m, 35m, 50m, 90m, 160m, 250m);
        AddStandardPackages(standardPackages, VehicleType.Van, 25m, 45m, 65m, 120m, 200m, 300m);
        AddStandardPackages(standardPackages, VehicleType.Bus, 35m, 60m, 90m, 160m, 280m, 420m);
        AddStandardPackages(standardPackages, VehicleType.Truck, 40m, 70m, 100m, 180m, 320m, 480m);

        var existingPackages = await db.ParkingPackages
            .Select(p => new { p.VehicleType, p.Name })
            .ToListAsync();

        var missingPackages = standardPackages
            .Where(s => !existingPackages.Any(e => e.VehicleType == s.VehicleType && e.Name == s.Name))
            .ToList();

        if (missingPackages.Count > 0)
        {
            db.ParkingPackages.AddRange(missingPackages);
            await db.SaveChangesAsync();
        }
    }

    private static void AddStandardPackages(List<ParkingPackage> packages, VehicleType type,
        decimal oneHour, decimal twoHours, decimal threeHours, decimal sixHours, decimal twelveHours, decimal fullDay)
    {
        packages.Add(new ParkingPackage { VehicleType = type, Name = "1 Hour", DurationMinutes = 60, Price = oneHour });
        packages.Add(new ParkingPackage { VehicleType = type, Name = "2 Hours", DurationMinutes = 120, Price = twoHours });
        packages.Add(new ParkingPackage { VehicleType = type, Name = "3 Hours", DurationMinutes = 180, Price = threeHours });
        packages.Add(new ParkingPackage { VehicleType = type, Name = "6 Hours", DurationMinutes = 360, Price = sixHours });
        packages.Add(new ParkingPackage { VehicleType = type, Name = "12 Hours", DurationMinutes = 720, Price = twelveHours });
        packages.Add(new ParkingPackage { VehicleType = type, Name = "Full Day", DurationMinutes = 1440, Price = fullDay });
    }
}
