using GestionOpe.Domain.Common;

namespace GestionOpe.Domain.Entities;

public class Carrier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Rating { get; set; } = 5.0;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
