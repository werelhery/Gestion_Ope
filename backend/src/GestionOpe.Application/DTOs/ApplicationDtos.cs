using GestionOpe.Domain.Enums;

namespace GestionOpe.Application.DTOs;

public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude
);

public record DimensionsDto(
    double LengthCm,
    double WidthCm,
    double HeightCm,
    double WeightKg,
    double VolumeM3
);

public record ShipmentEventDto(
    Guid Id,
    ShipmentStatus Status,
    string Location,
    string Description,
    DateTime TimestampUtc
);

public record ShipmentDto(
    Guid Id,
    string TrackingNumber,
    string Description,
    string SenderName,
    string RecipientName,
    AddressDto OriginAddress,
    AddressDto DestinationAddress,
    Guid OriginWarehouseId,
    string OriginWarehouseName,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseName,
    Guid? CarrierId,
    string? CarrierName,
    ShipmentStatus Status,
    ShipmentPriority Priority,
    DimensionsDto Dimensions,
    bool IsTemperatureControlled,
    double? RequiredTemperatureCelsius,
    DateTime ScheduledPickupDateUtc,
    DateTime EstimatedDeliveryDateUtc,
    DateTime? ActualDeliveryDateUtc,
    bool IsDelayed,
    int IncidentCount,
    DateTime CreatedAtUtc
);

public record ShipmentDetailDto(
    Guid Id,
    string TrackingNumber,
    string Description,
    string SenderName,
    string RecipientName,
    AddressDto OriginAddress,
    AddressDto DestinationAddress,
    Guid OriginWarehouseId,
    string OriginWarehouseName,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseName,
    Guid? CarrierId,
    string? CarrierName,
    ShipmentStatus Status,
    ShipmentPriority Priority,
    DimensionsDto Dimensions,
    bool IsTemperatureControlled,
    double? RequiredTemperatureCelsius,
    DateTime ScheduledPickupDateUtc,
    DateTime EstimatedDeliveryDateUtc,
    DateTime? ActualDeliveryDateUtc,
    bool IsDelayed,
    int IncidentCount,
    DateTime CreatedAtUtc,
    IReadOnlyList<ShipmentEventDto> Events,
    IReadOnlyList<IncidentDto> Incidents
);

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    AddressDto Address,
    double TotalCapacityM3,
    double UsedCapacityM3,
    double CapacityUtilizationPercentage,
    WarehouseStatus Status,
    string ManagerName,
    string ContactEmail,
    string ContactPhone
);

public record CarrierDto(
    Guid Id,
    string Code,
    string Name,
    double Rating,
    string ContactEmail,
    string ContactPhone,
    bool IsActive
);

public record IncidentDto(
    Guid Id,
    Guid ShipmentId,
    string ShipmentTrackingNumber,
    string Code,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTime ReportedAtUtc,
    DateTime? ResolvedAtUtc,
    string ResolutionNotes
);

public record DashboardMetricsDto(
    int TotalShipments,
    int InTransitShipments,
    int DeliveredShipments,
    int DelayedShipments,
    double OnTimeDeliveryRatePercentage,
    int TotalWarehouses,
    double AverageWarehouseUtilizationPercentage,
    int ActiveIncidentsCount,
    IReadOnlyList<ShipmentDto> RecentShipments,
    IReadOnlyList<IncidentDto> UrgentIncidents
);

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    DateTime? LastLoginAtUtc
);

public record AuthResponseDto(
    string Token,
    UserDto User,
    DateTime ExpiresAtUtc
);
