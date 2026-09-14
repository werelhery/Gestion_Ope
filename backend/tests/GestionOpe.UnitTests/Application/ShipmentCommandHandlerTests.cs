using FluentAssertions;
using FluentValidation.TestHelper;
using GestionOpe.Application.DTOs;
using GestionOpe.Application.Features.Shipments.Commands;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Interfaces;
using GestionOpe.Domain.ValueObjects;
using Moq;
using Xunit;

namespace GestionOpe.UnitTests.Application;

public class ShipmentCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepoMock = new();
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateShipmentCommandHandler_Should_Create_Shipment_When_Valid()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Hub Paris",
            Code = "WH-PARIS"
        };

        _warehouseRepoMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var handler = new CreateShipmentCommandHandler(
            _shipmentRepoMock.Object,
            _warehouseRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateShipmentCommand(
            Description: "Lots informatiques",
            SenderName: "Dell Technologies",
            RecipientName: "BNP Paribas",
            OriginAddress: new AddressDto("10 Rue de la Paix", "Paris", "75001", "France", 48.8, 2.3),
            DestinationAddress: new AddressDto("1 Boulevard Haussmann", "Paris", "75009", "France", 48.87, 2.33),
            OriginWarehouseId: warehouseId,
            DestinationWarehouseId: null,
            CarrierId: null,
            Priority: ShipmentPriority.High,
            Dimensions: new DimensionsDto(80, 60, 40, 25, 0.192),
            IsTemperatureControlled: false,
            RequiredTemperatureCelsius: null,
            ScheduledPickupDateUtc: DateTime.UtcNow.AddHours(1),
            EstimatedDeliveryDateUtc: DateTime.UtcNow.AddHours(12)
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TrackingNumber.Should().StartWith("FR-EXP-");
        result.Status.Should().Be(ShipmentStatus.Scheduled);
        result.SenderName.Should().Be("Dell Technologies");

        _shipmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Shipment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CreateShipmentCommandValidator_Should_Fail_When_Weight_Is_Zero_Or_Negative()
    {
        // Arrange
        var validator = new CreateShipmentCommandValidator();
        var command = new CreateShipmentCommand(
            Description: "Test",
            SenderName: "Sender",
            RecipientName: "Recipient",
            OriginAddress: new AddressDto("Street", "City", "75000", "France", 0, 0),
            DestinationAddress: new AddressDto("Street", "City", "75000", "France", 0, 0),
            OriginWarehouseId: Guid.NewGuid(),
            DestinationWarehouseId: null,
            CarrierId: null,
            Priority: ShipmentPriority.Normal,
            Dimensions: new DimensionsDto(10, 10, 10, 0, 0), // Invalid weight 0
            IsTemperatureControlled: false,
            RequiredTemperatureCelsius: null,
            ScheduledPickupDateUtc: DateTime.UtcNow,
            EstimatedDeliveryDateUtc: DateTime.UtcNow.AddHours(2)
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Dimensions.WeightKg);
    }

    [Fact]
    public void CreateShipmentCommandValidator_Should_Fail_When_DeliveryDate_Precedes_PickupDate()
    {
        // Arrange
        var validator = new CreateShipmentCommandValidator();
        var command = new CreateShipmentCommand(
            Description: "Test",
            SenderName: "Sender",
            RecipientName: "Recipient",
            OriginAddress: new AddressDto("Street", "City", "75000", "France", 0, 0),
            DestinationAddress: new AddressDto("Street", "City", "75000", "France", 0, 0),
            OriginWarehouseId: Guid.NewGuid(),
            DestinationWarehouseId: null,
            CarrierId: null,
            Priority: ShipmentPriority.Normal,
            Dimensions: new DimensionsDto(10, 10, 10, 15, 0.001),
            IsTemperatureControlled: false,
            RequiredTemperatureCelsius: null,
            ScheduledPickupDateUtc: DateTime.UtcNow.AddDays(1),
            EstimatedDeliveryDateUtc: DateTime.UtcNow // Precedes Pickup!
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EstimatedDeliveryDateUtc);
    }
}
