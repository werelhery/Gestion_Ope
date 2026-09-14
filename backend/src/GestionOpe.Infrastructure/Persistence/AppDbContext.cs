using GestionOpe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionOpe.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentEvent> ShipmentEvents => Set<ShipmentEvent>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        });

        // Warehouse Configuration
        modelBuilder.Entity<Warehouse>(builder =>
        {
            builder.HasKey(w => w.Id);
            builder.HasIndex(w => w.Code).IsUnique();
            builder.Property(w => w.Code).HasMaxLength(50).IsRequired();
            builder.Property(w => w.Name).HasMaxLength(150).IsRequired();

            builder.OwnsOne(w => w.Address, a =>
            {
                a.Property(p => p.Street).HasMaxLength(200);
                a.Property(p => p.City).HasMaxLength(100);
                a.Property(p => p.PostalCode).HasMaxLength(20);
                a.Property(p => p.Country).HasMaxLength(100);
            });
        });

        // Carrier Configuration
        modelBuilder.Entity<Carrier>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.Code).IsUnique();
            builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        });

        // Shipment Configuration
        modelBuilder.Entity<Shipment>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.TrackingNumber).IsUnique();
            builder.Property(s => s.TrackingNumber).HasMaxLength(50).IsRequired();
            builder.Property(s => s.SenderName).HasMaxLength(150).IsRequired();
            builder.Property(s => s.RecipientName).HasMaxLength(150).IsRequired();

            builder.OwnsOne(s => s.OriginAddress, a =>
            {
                a.Property(p => p.Street).HasMaxLength(200);
                a.Property(p => p.City).HasMaxLength(100);
                a.Property(p => p.PostalCode).HasMaxLength(20);
                a.Property(p => p.Country).HasMaxLength(100);
            });

            builder.OwnsOne(s => s.DestinationAddress, a =>
            {
                a.Property(p => p.Street).HasMaxLength(200);
                a.Property(p => p.City).HasMaxLength(100);
                a.Property(p => p.PostalCode).HasMaxLength(20);
                a.Property(p => p.Country).HasMaxLength(100);
            });

            builder.OwnsOne(s => s.Dimensions, d =>
            {
                d.Ignore(dim => dim.VolumeM3);
            });

            builder.HasOne(s => s.OriginWarehouse)
                .WithMany(w => w.OriginatingShipments)
                .HasForeignKey(s => s.OriginWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.DestinationWarehouse)
                .WithMany(w => w.IncomingShipments)
                .HasForeignKey(s => s.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(s => s.Carrier)
                .WithMany(c => c.Shipments)
                .HasForeignKey(s => s.CarrierId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(s => s.Events)
                .WithOne(e => e.Shipment)
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Incidents)
                .WithOne(i => i.Shipment)
                .HasForeignKey(i => i.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Incident Configuration
        modelBuilder.Entity<Incident>(builder =>
        {
            builder.HasKey(i => i.Id);
            builder.HasIndex(i => i.Code).IsUnique();
            builder.Property(i => i.Code).HasMaxLength(50).IsRequired();
            builder.Property(i => i.Title).HasMaxLength(200).IsRequired();
        });

        // ShipmentEvent Configuration
        modelBuilder.Entity<ShipmentEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Location).HasMaxLength(150);
            builder.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
