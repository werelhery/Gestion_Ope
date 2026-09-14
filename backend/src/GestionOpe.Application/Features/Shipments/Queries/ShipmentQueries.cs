using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Shipments.Queries;

public record GetShipmentsQuery(
    ShipmentStatus? Status = null,
    ShipmentPriority? Priority = null,
    string? Search = null
) : IRequest<IReadOnlyList<ShipmentDto>>;

public class GetShipmentsQueryHandler : IRequestHandler<GetShipmentsQuery, IReadOnlyList<ShipmentDto>>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetShipmentsQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<IReadOnlyList<ShipmentDto>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var shipments = await _shipmentRepository.GetAllAsync(
            request.Status,
            request.Priority,
            request.Search,
            cancellationToken
        );

        return shipments.Select(s => s.ToDto()).ToList();
    }
}

public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDetailDto>;

public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDetailDto>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetShipmentByIdQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<ShipmentDetailDto> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (shipment == null)
        {
            throw new ShipmentNotFoundException(request.Id);
        }

        return shipment.ToDetailDto();
    }
}

public record GetShipmentByTrackingNumberQuery(string TrackingNumber) : IRequest<ShipmentDetailDto>;

public class GetShipmentByTrackingNumberQueryHandler : IRequestHandler<GetShipmentByTrackingNumberQuery, ShipmentDetailDto>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetShipmentByTrackingNumberQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<ShipmentDetailDto> Handle(GetShipmentByTrackingNumberQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByTrackingNumberAsync(request.TrackingNumber, cancellationToken);
        if (shipment == null)
        {
            throw new ShipmentNotFoundException(request.TrackingNumber);
        }

        return shipment.ToDetailDto();
    }
}
