using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;

namespace GestionOpe.Domain.Interfaces;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentStatus? status = null, ShipmentPriority? priority = null, string? search = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ShipmentStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountDelayedAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    void Update(Shipment shipment);
    void Delete(Shipment shipment);
}

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    void Update(Warehouse warehouse);
}

public interface ICarrierRepository
{
    Task<Carrier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Carrier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Carrier carrier, CancellationToken cancellationToken = default);
}

public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetOpenIncidentsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Incident incident, CancellationToken cancellationToken = default);
    void Update(Incident incident);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
