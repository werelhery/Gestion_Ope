using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionOpe.Infrastructure.Persistence.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.OriginWarehouse)
            .Include(s => s.DestinationWarehouse)
            .Include(s => s.Carrier)
            .Include(s => s.Events)
            .Include(s => s.Incidents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.OriginWarehouse)
            .Include(s => s.DestinationWarehouse)
            .Include(s => s.Carrier)
            .Include(s => s.Events)
            .Include(s => s.Incidents)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Shipment>> GetAllAsync(
        ShipmentStatus? status = null, 
        ShipmentPriority? priority = null, 
        string? search = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Shipments
            .Include(s => s.OriginWarehouse)
            .Include(s => s.DestinationWarehouse)
            .Include(s => s.Carrier)
            .Include(s => s.Incidents)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(s => s.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower().Trim();
            query = query.Where(x => 
                x.TrackingNumber.ToLower().Contains(s) ||
                x.SenderName.ToLower().Contains(s) ||
                x.RecipientName.ToLower().Contains(s) ||
                x.Description.ToLower().Contains(s) ||
                x.OriginAddress.City.ToLower().Contains(s) ||
                x.DestinationAddress.City.ToLower().Contains(s));
        }

        return await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ShipmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        if (status.HasValue)
        {
            return await _context.Shipments.CountAsync(s => s.Status == status.Value, cancellationToken);
        }

        return await _context.Shipments.CountAsync(cancellationToken);
    }

    public async Task<int> CountDelayedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Shipments.CountAsync(s => 
            s.Status != ShipmentStatus.Delivered && 
            s.Status != ShipmentStatus.Cancelled && 
            s.EstimatedDeliveryDateUtc < now, 
            cancellationToken);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _context.Shipments.AddAsync(shipment, cancellationToken);
    }

    public void Update(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
    }

    public void Delete(Shipment shipment)
    {
        _context.Shipments.Remove(shipment);
    }
}

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _context;

    public WarehouseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _context.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    public void Update(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
    }
}

public class CarrierRepository : ICarrierRepository
{
    private readonly AppDbContext _context;

    public CarrierRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Carrier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carriers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Carrier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Carriers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Carrier carrier, CancellationToken cancellationToken = default)
    {
        await _context.Carriers.AddAsync(carrier, cancellationToken);
    }
}

public class IncidentRepository : IIncidentRepository
{
    private readonly AppDbContext _context;

    public IncidentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Incidents
            .Include(i => i.Shipment)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Incidents
            .Include(i => i.Shipment)
            .Where(i => i.ShipmentId == shipmentId)
            .OrderByDescending(i => i.ReportedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetOpenIncidentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Incidents
            .Include(i => i.Shipment)
            .Where(i => i.Status != IncidentStatus.Resolved)
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.ReportedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await _context.Incidents.AddAsync(incident, cancellationToken);
    }

    public void Update(Incident incident)
    {
        _context.Incidents.Update(incident);
    }
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
