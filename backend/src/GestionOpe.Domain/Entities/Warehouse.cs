using GestionOpe.Domain.Common;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.ValueObjects;

namespace GestionOpe.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public double TotalCapacityM3 { get; set; }
    public double UsedCapacityM3 { get; set; }
    public WarehouseStatus Status { get; set; } = WarehouseStatus.Active;
    public string ManagerName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public double CapacityUtilizationPercentage => TotalCapacityM3 > 0 
        ? Math.Round((UsedCapacityM3 / TotalCapacityM3) * 100, 2) 
        : 0;

    public ICollection<Shipment> OriginatingShipments { get; set; } = new List<Shipment>();
    public ICollection<Shipment> IncomingShipments { get; set; } = new List<Shipment>();
}
