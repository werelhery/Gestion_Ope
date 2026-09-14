using GestionOpe.Application.Interfaces;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.ValueObjects;
using GestionOpe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionOpe.Infrastructure.Persistence.Seeding;

public static class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            if (await context.Warehouses.AnyAsync())
            {
                return; // Already seeded
            }

            logger.LogInformation("🌱 Seeding initial operational logistics data...");

            // 1. Users
            var adminUser = new User
            {
                Email = "admin@gestionope.fr",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                FirstName = "Hery",
                LastName = "Rajaona",
                Role = UserRole.Admin,
                IsActive = true
            };

            var managerUser = new User
            {
                Email = "manager@gestionope.fr",
                PasswordHash = passwordHasher.HashPassword("Manager123!"),
                FirstName = "Sophie",
                LastName = "Laurent",
                Role = UserRole.LogisticsManager,
                IsActive = true
            };

            var operatorUser = new User
            {
                Email = "operator@gestionope.fr",
                PasswordHash = passwordHasher.HashPassword("Operator123!"),
                FirstName = "Lucas",
                LastName = "Moreau",
                Role = UserRole.WarehouseOperator,
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, managerUser, operatorUser);

            // 2. Warehouses
            var whParis = new Warehouse
            {
                Code = "WH-PARIS-NORD",
                Name = "Hub Logistique Paris-Nord (Roissy)",
                Address = new Address("12 Rue du Fret Aérien", "Roissy-en-France", "95700", "France", 49.0097, 2.5479),
                TotalCapacityM3 = 50000,
                UsedCapacityM3 = 38500,
                Status = WarehouseStatus.Active,
                ManagerName = "Marc Dubois",
                ContactEmail = "paris.nord@gestionope.fr",
                ContactPhone = "+33 1 48 62 00 01"
            };

            var whLyon = new Warehouse
            {
                Code = "WH-LYON-EXUPERY",
                Name = "Plateforme Fret Lyon Saint-Exupéry",
                Address = new Address("45 Avenue de l'Europe", "Colombier-Saugnieu", "69125", "France", 45.7256, 5.0811),
                TotalCapacityM3 = 35000,
                UsedCapacityM3 = 24000,
                Status = WarehouseStatus.Active,
                ManagerName = "Claire Vasseur",
                ContactEmail = "lyon.hub@gestionope.fr",
                ContactPhone = "+33 4 72 22 10 20"
            };

            var whMarseille = new Warehouse
            {
                Code = "WH-MARSEILLE-FOS",
                Name = "Terminal Maritime & Fret Marseille-Fos",
                Address = new Address("Route du Môle Central", "Fos-sur-Mer", "13270", "France", 43.4389, 4.9458),
                TotalCapacityM3 = 60000,
                UsedCapacityM3 = 52000,
                Status = WarehouseStatus.Active,
                ManagerName = "Karim Benali",
                ContactEmail = "marseille.port@gestionope.fr",
                ContactPhone = "+33 4 91 99 30 40"
            };

            var whLille = new Warehouse
            {
                Code = "WH-LILLE-EURO",
                Name = "Centre Distribution Lille Métropole",
                Address = new Address("8 Boulevard de l'Industrie", "Lesquin", "59810", "France", 50.5833, 3.1000),
                TotalCapacityM3 = 28000,
                UsedCapacityM3 = 19200,
                Status = WarehouseStatus.Active,
                ManagerName = "Élodie Martin",
                ContactEmail = "lille.hub@gestionope.fr",
                ContactPhone = "+33 3 20 16 70 80"
            };

            await context.Warehouses.AddRangeAsync(whParis, whLyon, whMarseille, whLille);

            // 3. Carriers
            var cTransEurope = new Carrier
            {
                Code = "CAR-TEE",
                Name = "TransEurope Express Lines",
                Rating = 4.8,
                ContactEmail = "dispatch@transeurope.eu",
                ContactPhone = "+33 1 80 50 40 30",
                IsActive = true
            };

            var cChronoFret = new Carrier
            {
                Code = "CAR-C24",
                Name = "ChronoFret 24/7",
                Rating = 4.6,
                ContactEmail = "operations@chronofret24.com",
                ContactPhone = "+33 4 78 90 12 34",
                IsActive = true
            };

            var cGreenLine = new Carrier
            {
                Code = "CAR-GLT",
                Name = "GreenLine Eco-Transport (Biogaz & Électrique)",
                Rating = 4.9,
                ContactEmail = "contact@greenline-eco.fr",
                ContactPhone = "+33 2 40 12 34 56",
                IsActive = true
            };

            await context.Carriers.AddRangeAsync(cTransEurope, cChronoFret, cGreenLine);

            // 4. Shipments
            var now = DateTime.UtcNow;

            var s1 = new Shipment
            {
                TrackingNumber = "FR-EXP-2026-0891",
                Description = "Palettes pharmaceutiques sous température dirigée (+4°C)",
                SenderName = "Sanofi Pasteur Lyon",
                RecipientName = "Pharmacie Centrale AP-HP Paris",
                OriginAddress = new Address("Campus Mérieux", "Lyon", "69007", "France", 45.7300, 4.8300),
                DestinationAddress = new Address("Hôpital Bichat", "Paris", "75018", "France", 48.8990, 2.3320),
                OriginWarehouse = whLyon,
                DestinationWarehouse = whParis,
                Carrier = cTransEurope,
                Status = ShipmentStatus.InTransit,
                Priority = ShipmentPriority.Critical,
                Dimensions = new Dimensions(120, 80, 160, 450),
                IsTemperatureControlled = true,
                RequiredTemperatureCelsius = 4.0,
                ScheduledPickupDateUtc = now.AddHours(-6),
                EstimatedDeliveryDateUtc = now.AddHours(4),
            };
            s1.Events.Add(new ShipmentEvent { Shipment = s1, Status = ShipmentStatus.Scheduled, Location = "Lyon", Description = "Prise en charge validée", TimestampUtc = now.AddHours(-6) });
            s1.Events.Add(new ShipmentEvent { Shipment = s1, Status = ShipmentStatus.InTransit, Location = "Autoroute A6 (Beaune)", Description = "En transit routier fluide", TimestampUtc = now.AddHours(-2) });

            var s2 = new Shipment
            {
                TrackingNumber = "FR-EXP-2026-0892",
                Description = "Composants électroniques aéronautiques haute précision",
                SenderName = "Safran Nacelles Le Havre",
                RecipientName = "Airbus Operations Toulouse",
                OriginAddress = new Address("Zone Industrielle", "Gonfreville-l'Orcher", "76700", "France", 49.4900, 0.2200),
                DestinationAddress = new Address("Site Clément Ader", "Toulouse", "31770", "France", 43.6290, 1.3630),
                OriginWarehouse = whParis,
                DestinationWarehouse = whMarseille,
                Carrier = cChronoFret,
                Status = ShipmentStatus.OutForDelivery,
                Priority = ShipmentPriority.High,
                Dimensions = new Dimensions(100, 60, 80, 120),
                IsTemperatureControlled = false,
                ScheduledPickupDateUtc = now.AddHours(-18),
                EstimatedDeliveryDateUtc = now.AddHours(2),
            };
            s2.Events.Add(new ShipmentEvent { Shipment = s2, Status = ShipmentStatus.Scheduled, Location = "Paris-Nord", Description = "Colis enregistré", TimestampUtc = now.AddHours(-18) });
            s2.Events.Add(new ShipmentEvent { Shipment = s2, Status = ShipmentStatus.InTransit, Location = "Centre de Tri Régional", Description = "Tri automatisé terminé", TimestampUtc = now.AddHours(-8) });
            s2.Events.Add(new ShipmentEvent { Shipment = s2, Status = ShipmentStatus.OutForDelivery, Location = "Toulouse Métropole", Description = "En cours de livraison au destinataire", TimestampUtc = now.AddHours(-1) });

            var s3 = new Shipment
            {
                TrackingNumber = "FR-EXP-2026-0893",
                Description = "Conteneur pièces détachées industrielles maritimes",
                SenderName = "ArcelorMittal Dunkerque",
                RecipientName = "Chantiers Navals de Marseille",
                OriginAddress = new Address("Port Ouest", "Dunkerque", "59140", "France", 51.0340, 2.3760),
                DestinationAddress = new Address("Forme 10 Port", "Marseille", "13002", "France", 43.3450, 5.3480),
                OriginWarehouse = whLille,
                DestinationWarehouse = whMarseille,
                Carrier = cTransEurope,
                Status = ShipmentStatus.Delayed,
                Priority = ShipmentPriority.High,
                Dimensions = new Dimensions(605, 243, 259, 12500),
                IsTemperatureControlled = false,
                ScheduledPickupDateUtc = now.AddDays(-2),
                EstimatedDeliveryDateUtc = now.AddHours(-4), // In the past -> Delayed!
            };
            s3.Events.Add(new ShipmentEvent { Shipment = s3, Status = ShipmentStatus.InTransit, Location = "Lille", Description = "Départ convoi lourd", TimestampUtc = now.AddDays(-2) });
            s3.Events.Add(new ShipmentEvent { Shipment = s3, Status = ShipmentStatus.Delayed, Location = "Valence Sud", Description = "Ralentissement majeur incident autoroutier", TimestampUtc = now.AddHours(-5) });

            var inc1 = new Incident
            {
                Shipment = s3,
                Code = "INC-2609-001",
                Title = "Blocage axe autoroutier suite à intempéries",
                Description = "Fermeture partielle de l'axe A7 près de Valence suite à des rafales et débris sur chaussée. Retard estimé de 6h.",
                Severity = IncidentSeverity.Major,
                Status = IncidentStatus.Open,
                ReportedAtUtc = now.AddHours(-5)
            };
            s3.Incidents.Add(inc1);

            var s4 = new Shipment
            {
                TrackingNumber = "FR-EXP-2026-0894",
                Description = "Lots textile & prêt-à-porter éco-responsable",
                SenderName = "Decathlon Logistique",
                RecipientName = "Magasin Flagship Paris Madeleine",
                OriginAddress = new Address("Entrepôt Nord", "Dourges", "62119", "France", 50.4370, 2.9830),
                DestinationAddress = new Address("Place de la Madeleine", "Paris", "75008", "France", 48.8700, 2.3240),
                OriginWarehouse = whLille,
                DestinationWarehouse = whParis,
                Carrier = cGreenLine,
                Status = ShipmentStatus.Delivered,
                Priority = ShipmentPriority.Normal,
                Dimensions = new Dimensions(80, 60, 50, 45),
                IsTemperatureControlled = false,
                ScheduledPickupDateUtc = now.AddDays(-1),
                EstimatedDeliveryDateUtc = now.AddHours(-2),
                ActualDeliveryDateUtc = now.AddHours(-2).AddMinutes(15)
            };
            s4.Events.Add(new ShipmentEvent { Shipment = s4, Status = ShipmentStatus.Scheduled, Location = "Lille Hub", Description = "Colis scanné", TimestampUtc = now.AddDays(-1) });
            s4.Events.Add(new ShipmentEvent { Shipment = s4, Status = ShipmentStatus.InTransit, Location = "A1 Senlis", Description = "En acheminement décarboné", TimestampUtc = now.AddHours(-6) });
            s4.Events.Add(new ShipmentEvent { Shipment = s4, Status = ShipmentStatus.Delivered, Location = "Paris Madeleine", Description = "Livré avec signature électronique", TimestampUtc = now.AddHours(-2).AddMinutes(15) });

            var s5 = new Shipment
            {
                TrackingNumber = "FR-EXP-2026-0895",
                Description = "Produits agroalimentaires bio locaux",
                SenderName = "Coopérative Provence Terroir",
                RecipientName = "Marché International de Rungis",
                OriginAddress = new Address("Chemin des Oliviers", "Cavaillon", "84300", "France", 43.8340, 5.0370),
                DestinationAddress = new Address("Pavillon Bio", "Rungis", "94150", "France", 48.7490, 2.3530),
                OriginWarehouse = whMarseille,
                DestinationWarehouse = whParis,
                Carrier = cGreenLine,
                Status = ShipmentStatus.Scheduled,
                Priority = ShipmentPriority.Normal,
                Dimensions = new Dimensions(120, 80, 140, 320),
                IsTemperatureControlled = true,
                RequiredTemperatureCelsius = 6.0,
                ScheduledPickupDateUtc = now.AddHours(2),
                EstimatedDeliveryDateUtc = now.AddHours(14),
            };

            await context.Shipments.AddRangeAsync(s1, s2, s3, s4, s5);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Database successfully seeded with 4 Hubs, 3 Carriers, 5 Shipments and demo users.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database seeding: {Message}", ex.Message);
        }
    }
}
