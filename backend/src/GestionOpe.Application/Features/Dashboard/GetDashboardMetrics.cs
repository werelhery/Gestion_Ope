using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Dashboard;

public record GetDashboardMetricsQuery : IRequest<DashboardMetricsDto>;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IIncidentRepository _incidentRepository;

    public GetDashboardMetricsQueryHandler(
        IShipmentRepository shipmentRepository,
        IWarehouseRepository warehouseRepository,
        IIncidentRepository incidentRepository)
    {
        _shipmentRepository = shipmentRepository;
        _warehouseRepository = warehouseRepository;
        _incidentRepository = incidentRepository;
    }

    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var totalShipments = await _shipmentRepository.CountAsync(cancellationToken: cancellationToken);
        var inTransit = await _shipmentRepository.CountAsync(ShipmentStatus.InTransit, cancellationToken);
        var delivered = await _shipmentRepository.CountAsync(ShipmentStatus.Delivered, cancellationToken);
        var delayed = await _shipmentRepository.CountDelayedAsync(cancellationToken);

        var onTimeRate = (totalShipments > 0)
            ? Math.Round(((double)(totalShipments - delayed) / totalShipments) * 100, 1)
            : 100.0;

        var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
        var avgUtilization = warehouses.Any()
            ? Math.Round(warehouses.Average(w => w.CapacityUtilizationPercentage), 1)
            : 0.0;

        var openIncidents = await _incidentRepository.GetOpenIncidentsAsync(cancellationToken);
        var recentShipments = await _shipmentRepository.GetAllAsync(cancellationToken: cancellationToken);

        return new DashboardMetricsDto(
            TotalShipments: totalShipments,
            InTransitShipments: inTransit,
            DeliveredShipments: delivered,
            DelayedShipments: delayed,
            OnTimeDeliveryRatePercentage: onTimeRate,
            TotalWarehouses: warehouses.Count,
            AverageWarehouseUtilizationPercentage: avgUtilization,
            ActiveIncidentsCount: openIncidents.Count,
            RecentShipments: recentShipments.Take(10).Select(s => s.ToDto()).ToList(),
            UrgentIncidents: openIncidents.Take(5).Select(i => i.ToDto()).ToList()
        );
    }
}
