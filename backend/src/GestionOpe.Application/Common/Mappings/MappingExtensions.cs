using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.ValueObjects;

namespace GestionOpe.Application.Common.Mappings;

public static class MappingExtensions
{
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            address.Street,
            address.City,
            address.PostalCode,
            address.Country,
            address.Latitude,
            address.Longitude
        );
    }

    public static Address ToDomain(this AddressDto dto)
    {
        return new Address(
            dto.Street,
            dto.City,
            dto.PostalCode,
            dto.Country,
            dto.Latitude,
            dto.Longitude
        );
    }

    public static DimensionsDto ToDto(this Dimensions dim)
    {
        return new DimensionsDto(
            dim.LengthCm,
            dim.WidthCm,
            dim.HeightCm,
            dim.WeightKg,
            dim.VolumeM3
        );
    }

    public static Dimensions ToDomain(this DimensionsDto dto)
    {
        return new Dimensions(
            dto.LengthCm,
            dto.WidthCm,
            dto.HeightCm,
            dto.WeightKg
        );
    }

    public static ShipmentDto ToDto(this Shipment s)
    {
        return new ShipmentDto(
            s.Id,
            s.TrackingNumber,
            s.Description,
            s.SenderName,
            s.RecipientName,
            s.OriginAddress.ToDto(),
            s.DestinationAddress.ToDto(),
            s.OriginWarehouseId,
            s.OriginWarehouse?.Name ?? "N/A",
            s.DestinationWarehouseId,
            s.DestinationWarehouse?.Name,
            s.CarrierId,
            s.Carrier?.Name,
            s.Status,
            s.Priority,
            s.Dimensions.ToDto(),
            s.IsTemperatureControlled,
            s.RequiredTemperatureCelsius,
            s.ScheduledPickupDateUtc,
            s.EstimatedDeliveryDateUtc,
            s.ActualDeliveryDateUtc,
            s.IsDelayed,
            s.Incidents?.Count ?? 0,
            s.CreatedAtUtc
        );
    }

    public static ShipmentDetailDto ToDetailDto(this Shipment s)
    {
        return new ShipmentDetailDto(
            s.Id,
            s.TrackingNumber,
            s.Description,
            s.SenderName,
            s.RecipientName,
            s.OriginAddress.ToDto(),
            s.DestinationAddress.ToDto(),
            s.OriginWarehouseId,
            s.OriginWarehouse?.Name ?? "N/A",
            s.DestinationWarehouseId,
            s.DestinationWarehouse?.Name,
            s.CarrierId,
            s.Carrier?.Name,
            s.Status,
            s.Priority,
            s.Dimensions.ToDto(),
            s.IsTemperatureControlled,
            s.RequiredTemperatureCelsius,
            s.ScheduledPickupDateUtc,
            s.EstimatedDeliveryDateUtc,
            s.ActualDeliveryDateUtc,
            s.IsDelayed,
            s.Incidents?.Count ?? 0,
            s.CreatedAtUtc,
            s.Events?.OrderByDescending(e => e.TimestampUtc).Select(e => new ShipmentEventDto(
                e.Id,
                e.Status,
                e.Location,
                e.Description,
                e.TimestampUtc
            )).ToList() ?? new List<ShipmentEventDto>(),
            s.Incidents?.Select(i => i.ToDto()).ToList() ?? new List<IncidentDto>()
        );
    }

    public static WarehouseDto ToDto(this Warehouse w)
    {
        return new WarehouseDto(
            w.Id,
            w.Code,
            w.Name,
            w.Address.ToDto(),
            w.TotalCapacityM3,
            w.UsedCapacityM3,
            w.CapacityUtilizationPercentage,
            w.Status,
            w.ManagerName,
            w.ContactEmail,
            w.ContactPhone
        );
    }

    public static CarrierDto ToDto(this Carrier c)
    {
        return new CarrierDto(
            c.Id,
            c.Code,
            c.Name,
            c.Rating,
            c.ContactEmail,
            c.ContactPhone,
            c.IsActive
        );
    }

    public static IncidentDto ToDto(this Incident i)
    {
        return new IncidentDto(
            i.Id,
            i.ShipmentId,
            i.Shipment?.TrackingNumber ?? string.Empty,
            i.Code,
            i.Title,
            i.Description,
            i.Severity,
            i.Status,
            i.ReportedAtUtc,
            i.ResolvedAtUtc,
            i.ResolutionNotes
        );
    }

    public static UserDto ToDto(this User u)
    {
        return new UserDto(
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.LastLoginAtUtc
        );
    }
}
