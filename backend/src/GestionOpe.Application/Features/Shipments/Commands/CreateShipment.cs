using FluentValidation;
using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Shipments.Commands;

public record CreateShipmentCommand(
    string Description,
    string SenderName,
    string RecipientName,
    AddressDto OriginAddress,
    AddressDto DestinationAddress,
    Guid OriginWarehouseId,
    Guid? DestinationWarehouseId,
    Guid? CarrierId,
    ShipmentPriority Priority,
    DimensionsDto Dimensions,
    bool IsTemperatureControlled,
    double? RequiredTemperatureCelsius,
    DateTime ScheduledPickupDateUtc,
    DateTime EstimatedDeliveryDateUtc
) : IRequest<ShipmentDto>;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SenderName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OriginAddress).NotNull();
        RuleFor(x => x.DestinationAddress).NotNull();
        RuleFor(x => x.OriginWarehouseId).NotEmpty();
        RuleFor(x => x.Dimensions).NotNull();
        RuleFor(x => x.Dimensions.WeightKg).GreaterThan(0).WithMessage("Le poids doit être supérieur à 0 kg.");
        RuleFor(x => x.EstimatedDeliveryDateUtc)
            .GreaterThan(x => x.ScheduledPickupDateUtc)
            .WithMessage("La date estimée de livraison doit être postérieure à la date de prise en charge.");
    }
}

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, ShipmentDto>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShipmentDto> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var originWarehouse = await _warehouseRepository.GetByIdAsync(request.OriginWarehouseId, cancellationToken);
        if (originWarehouse == null)
        {
            throw new WarehouseNotFoundException(request.OriginWarehouseId);
        }

        var trackingNumber = $"FR-EXP-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var shipment = new Shipment
        {
            TrackingNumber = trackingNumber,
            Description = request.Description,
            SenderName = request.SenderName,
            RecipientName = request.RecipientName,
            OriginAddress = request.OriginAddress.ToDomain(),
            DestinationAddress = request.DestinationAddress.ToDomain(),
            OriginWarehouseId = request.OriginWarehouseId,
            OriginWarehouse = originWarehouse,
            DestinationWarehouseId = request.DestinationWarehouseId,
            CarrierId = request.CarrierId,
            Priority = request.Priority,
            Status = ShipmentStatus.Scheduled,
            Dimensions = request.Dimensions.ToDomain(),
            IsTemperatureControlled = request.IsTemperatureControlled,
            RequiredTemperatureCelsius = request.RequiredTemperatureCelsius,
            ScheduledPickupDateUtc = request.ScheduledPickupDateUtc,
            EstimatedDeliveryDateUtc = request.EstimatedDeliveryDateUtc
        };

        shipment.Events.Add(new ShipmentEvent
        {
            ShipmentId = shipment.Id,
            Status = ShipmentStatus.Scheduled,
            Location = originWarehouse.Name,
            Description = "Expédition planifiée et enregistrée dans le système central.",
            TimestampUtc = DateTime.UtcNow
        });

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shipment.ToDto();
    }
}
