using FluentValidation;
using GestionOpe.Application.Common.Mappings;
using GestionOpe.Application.DTOs;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using MediatR;

namespace GestionOpe.Application.Features.Warehouses;

public record GetWarehousesQuery : IRequest<IReadOnlyList<WarehouseDto>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseDto>>
{
    private readonly IWarehouseRepository _warehouseRepository;

    public GetWarehousesQueryHandler(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<IReadOnlyList<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
        return warehouses.Select(w => w.ToDto()).ToList();
    }
}

public record CreateWarehouseCommand(
    string Code,
    string Name,
    AddressDto Address,
    double TotalCapacityM3,
    string ManagerName,
    string ContactEmail,
    string ContactPhone
) : IRequest<WarehouseDto>;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TotalCapacityM3).GreaterThan(0);
        RuleFor(x => x.Address).NotNull();
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            Address = request.Address.ToDomain(),
            TotalCapacityM3 = request.TotalCapacityM3,
            UsedCapacityM3 = 0,
            Status = WarehouseStatus.Active,
            ManagerName = request.ManagerName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        await _warehouseRepository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return warehouse.ToDto();
    }
}
