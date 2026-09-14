using FluentAssertions;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.ValueObjects;
using Xunit;

namespace GestionOpe.UnitTests.Domain;

public class ShipmentDomainTests
{
    [Fact]
    public void Dimensions_Should_Calculate_Correct_Volume_In_Cubic_Meters()
    {
        // Arrange (100cm x 100cm x 100cm = 1m³)
        var dimensions = new Dimensions(100, 100, 100, 50);

        // Act & Assert
        dimensions.VolumeM3.Should().Be(1.0);
    }

    [Fact]
    public void Warehouse_CapacityUtilization_Should_Calculate_Percentage_Accurately()
    {
        // Arrange
        var warehouse = new Warehouse
        {
            Code = "WH-TEST",
            Name = "Test Hub",
            TotalCapacityM3 = 10000,
            UsedCapacityM3 = 7500
        };

        // Act & Assert
        warehouse.CapacityUtilizationPercentage.Should().Be(75.0);
    }

    [Fact]
    public void Shipment_UpdateStatus_Should_Add_Event_And_Update_Status()
    {
        // Arrange
        var shipment = new Shipment
        {
            TrackingNumber = "FR-EXP-2026-TEST",
            Status = ShipmentStatus.Scheduled
        };

        // Act
        shipment.UpdateStatus(ShipmentStatus.InTransit, "Hub Paris", "Départ du camion");

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.InTransit);
        shipment.Events.Should().HaveCount(1);
        shipment.Events.First().Status.Should().Be(ShipmentStatus.InTransit);
        shipment.Events.First().Location.Should().Be("Hub Paris");
    }

    [Fact]
    public void Shipment_UpdateStatus_To_Delivered_Should_Set_ActualDeliveryDate()
    {
        // Arrange
        var shipment = new Shipment
        {
            TrackingNumber = "FR-EXP-2026-TEST",
            Status = ShipmentStatus.OutForDelivery
        };

        // Act
        shipment.UpdateStatus(ShipmentStatus.Delivered, "Client final", "Livraison effectuée avec succès");

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.Delivered);
        shipment.ActualDeliveryDateUtc.Should().NotBeNull();
        shipment.ActualDeliveryDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Shipment_UpdateStatus_When_Already_Delivered_Should_Throw_InvalidShipmentTransitionException()
    {
        // Arrange
        var shipment = new Shipment
        {
            TrackingNumber = "FR-EXP-2026-TEST",
            Status = ShipmentStatus.Delivered
        };

        // Act
        var act = () => shipment.UpdateStatus(ShipmentStatus.InTransit, "Paris", "Tentative de réouverture");

        // Assert
        act.Should().Throw<InvalidShipmentTransitionException>();
    }

    [Fact]
    public void Shipment_IsDelayed_Should_Return_True_When_Current_Time_Exceeds_EstimatedDeliveryDate()
    {
        // Arrange
        var shipment = new Shipment
        {
            Status = ShipmentStatus.InTransit,
            EstimatedDeliveryDateUtc = DateTime.UtcNow.AddHours(-2)
        };

        // Act & Assert
        shipment.IsDelayed.Should().BeTrue();
    }
}
