namespace GestionOpe.Domain.Enums;

public enum ShipmentStatus
{
    Draft = 0,
    Scheduled = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Delayed = 5,
    Cancelled = 6
}

public enum ShipmentPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum WarehouseStatus
{
    Active = 0,
    Full = 1,
    Maintenance = 2,
    Inactive = 3
}

public enum IncidentSeverity
{
    Minor = 0,
    Moderate = 1,
    Major = 2,
    Critical = 3
}

public enum IncidentStatus
{
    Open = 0,
    InInvestigation = 1,
    Mitigated = 2,
    Resolved = 3
}

public enum UserRole
{
    Admin = 0,
    LogisticsManager = 1,
    WarehouseOperator = 2,
    Dispatcher = 3
}
