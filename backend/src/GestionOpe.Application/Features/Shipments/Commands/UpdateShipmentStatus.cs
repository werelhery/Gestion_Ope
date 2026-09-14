using FluentValidation;
using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Shipments.Commands;

public record UpdateShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentStatus NewStatus,
    string Location,
    string Reason
) : IRequest<ShipmentDto>;

public class UpdateShipmentStatusCommandValidator : AbstractValidator<UpdateShipmentStatusCommand>
{
    public UpdateShipmentStatusCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(250);
    }
}

public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, ShipmentDto>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShipmentStatusCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShipmentDto> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null)
        {
            throw new ShipmentNotFoundException(request.ShipmentId);
        }

        shipment.UpdateStatus(request.NewStatus, request.Location, request.Reason);
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shipment.ToDto();
    }
}
