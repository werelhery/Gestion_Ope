using GestionOpe.Domain.Common;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.ValueObjects;

namespace GestionOpe.Domain.Entities;

public class Shipment : BaseEntity
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public Address OriginAddress { get; set; } = null!;
    public Address DestinationAddress { get; set; } = null!;

    public Guid OriginWarehouseId { get; set; }
    public Warehouse OriginWarehouse { get; set; } = null!;

    public Guid? DestinationWarehouseId { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }

    public Guid? CarrierId { get; set; }
    public Carrier? Carrier { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
    public ShipmentPriority Priority { get; set; } = ShipmentPriority.Normal;
    public Dimensions Dimensions { get; set; } = null!;

    public bool IsTemperatureControlled { get; set; }
    public double? RequiredTemperatureCelsius { get; set; }

    public DateTime ScheduledPickupDateUtc { get; set; }
    public DateTime EstimatedDeliveryDateUtc { get; set; }
    public DateTime? ActualDeliveryDateUtc { get; set; }

    public ICollection<ShipmentEvent> Events { get; set; } = new List<ShipmentEvent>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public bool IsDelayed => Status != ShipmentStatus.Delivered && 
                             Status != ShipmentStatus.Cancelled && 
                             DateTime.UtcNow > EstimatedDeliveryDateUtc;

    public void UpdateStatus(ShipmentStatus newStatus, string location, string reason)
    {
        if (Status == ShipmentStatus.Delivered || Status == ShipmentStatus.Cancelled)
        {
            throw new InvalidShipmentTransitionException(Status.ToString(), newStatus.ToString());
        }

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;

        if (newStatus == ShipmentStatus.Delivered)
        {
            ActualDeliveryDateUtc = DateTime.UtcNow;
        }

        Events.Add(new ShipmentEvent
        {
            ShipmentId = Id,
            Status = newStatus,
            Location = location,
            Description = reason,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
