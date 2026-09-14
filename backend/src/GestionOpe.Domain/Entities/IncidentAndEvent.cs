using GestionOpe.Domain.Common;
using GestionOpe.Domain.Enums;

namespace GestionOpe.Domain.Entities;

public class Incident : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Moderate;
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class ShipmentEvent : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public ShipmentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
