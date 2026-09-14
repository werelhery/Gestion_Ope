using GestionOpe.Application.DTOs;
using GestionOpe.Application.Features.Warehouses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOpe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetWarehousesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,LogisticsManager")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseCommand command)
    {
        var result = await _mediator.Send(command);
        return Created($"/api/warehouses/{result.Id}", result);
    }
}
