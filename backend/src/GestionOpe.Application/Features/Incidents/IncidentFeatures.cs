using FluentValidation;
using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Incidents;

public record GetIncidentsQuery(Guid? ShipmentId = null, bool OnlyOpen = false) : IRequest<IReadOnlyList<IncidentDto>>;

public class GetIncidentsQueryHandler : IRequestHandler<GetIncidentsQuery, IReadOnlyList<IncidentDto>>
{
    private readonly IIncidentRepository _incidentRepository;

    public GetIncidentsQueryHandler(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task<IReadOnlyList<IncidentDto>> Handle(GetIncidentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Incident> incidents;

        if (request.ShipmentId.HasValue)
        {
            incidents = await _incidentRepository.GetByShipmentIdAsync(request.ShipmentId.Value, cancellationToken);
        }
        else if (request.OnlyOpen)
        {
            incidents = await _incidentRepository.GetOpenIncidentsAsync(cancellationToken);
        }
        else
        {
            incidents = await _incidentRepository.GetOpenIncidentsAsync(cancellationToken);
        }

        return incidents.Select(i => i.ToDto()).ToList();
    }
}

public record CreateIncidentCommand(
    Guid ShipmentId,
    string Title,
    string Description,
    IncidentSeverity Severity
) : IRequest<IncidentDto>;

public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
    }
}

public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, IncidentDto>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _incidentRepository = incidentRepository;
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IncidentDto> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null)
        {
            throw new ShipmentNotFoundException(request.ShipmentId);
        }

        var incident = new Incident
        {
            ShipmentId = request.ShipmentId,
            Shipment = shipment,
            Code = $"INC-{DateTime.UtcNow:yyMMdd}-{new Random().Next(100, 999)}",
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Status = IncidentStatus.Open,
            ReportedAtUtc = DateTime.UtcNow
        };

        if (request.Severity >= IncidentSeverity.Major && shipment.Status != ShipmentStatus.Delivered)
        {
            shipment.Status = ShipmentStatus.Delayed;
            shipment.Events.Add(new ShipmentEvent
            {
                ShipmentId = shipment.Id,
                Status = ShipmentStatus.Delayed,
                Location = shipment.OriginWarehouse?.Name ?? "Hub",
                Description = $"Statut passé en RETARD suite à l'incident: {request.Title}",
                TimestampUtc = DateTime.UtcNow
            });
            _shipmentRepository.Update(shipment);
        }

        await _incidentRepository.AddAsync(incident, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return incident.ToDto();
    }
}
