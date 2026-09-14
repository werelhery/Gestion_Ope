namespace GestionOpe.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class ShipmentNotFoundException : DomainException
{
    public ShipmentNotFoundException(Guid id) : base($"Shipment with ID '{id}' was not found.") { }
    public ShipmentNotFoundException(string trackingNumber) : base($"Shipment with tracking number '{trackingNumber}' was not found.") { }
}

public class WarehouseNotFoundException : DomainException
{
    public WarehouseNotFoundException(Guid id) : base($"Warehouse with ID '{id}' was not found.") { }
}

public class InvalidShipmentTransitionException : DomainException
{
    public InvalidShipmentTransitionException(string fromStatus, string toStatus)
        : base($"Cannot transition shipment from status '{fromStatus}' to '{toStatus}'.") { }
}
