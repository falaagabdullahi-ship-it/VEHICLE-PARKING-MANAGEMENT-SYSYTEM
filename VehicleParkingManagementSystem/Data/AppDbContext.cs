using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleParkingManagementSystem.Models;

namespace VehicleParkingManagementSystem.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ParkingArea> ParkingAreas => Set<ParkingArea>();
    public DbSet<ParkingRecord> ParkingRecords => Set<ParkingRecord>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PricingSetting> PricingSettings => Set<PricingSetting>();
    public DbSet<ParkingPackage> ParkingPackages => Set<ParkingPackage>();
    public DbSet<ProfilePicture> ProfilePictures => Set<ProfilePicture>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Driver
        builder.Entity<Driver>(entity =>
        {
            entity.HasIndex(d => d.PhoneNumber);
            entity.HasOne(d => d.ApplicationUser)
                .WithOne()
                .HasForeignKey<Driver>(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Vehicle
        builder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(v => v.VehicleNumber).IsUnique();
            entity.HasOne(v => v.Driver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ParkingRecord
        builder.Entity<ParkingRecord>(entity =>
        {
            entity.Property(r => r.Fee).HasColumnType("decimal(10,2)");

            entity.HasOne(r => r.Vehicle)
                .WithMany(v => v.ParkingRecords)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ParkingArea)
                .WithMany()
                .HasForeignKey(r => r.ParkingAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.CheckedInByUser)
                .WithMany()
                .HasForeignKey(r => r.CheckedInByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.CheckedOutByUser)
                .WithMany()
                .HasForeignKey(r => r.CheckedOutByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Reservation)
                .WithOne(res => res.ParkingRecord)
                .HasForeignKey<ParkingRecord>(r => r.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.ParkingPackage)
                .WithMany()
                .HasForeignKey(r => r.ParkingPackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation
        builder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ParkingArea)
                .WithMany()
                .HasForeignKey(r => r.ParkingAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Notification
        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PricingSetting
        builder.Entity<PricingSetting>(entity =>
        {
            entity.HasIndex(p => p.VehicleType).IsUnique();
            entity.Property(p => p.HourlyRate).HasColumnType("decimal(10,2)");
            entity.Property(p => p.MinimumCharge).HasColumnType("decimal(10,2)");
        });

        // ParkingPackage
        builder.Entity<ParkingPackage>(entity =>
        {
            entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });

        // ProfilePicture
        builder.Entity<ProfilePicture>(entity =>
        {
            entity.HasKey(p => p.UserId);
            entity.Property(p => p.ContentType).HasMaxLength(50);
            entity.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<ProfilePicture>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
