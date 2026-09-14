using GestionOpe.Application.DTOs;
using GestionOpe.Application.Features.Shipments.Commands;
using GestionOpe.Application.Features.Shipments.Queries;
using GestionOpe.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOpe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShipmentDto>>> GetAll(
        [FromQuery] ShipmentStatus? status,
        [FromQuery] ShipmentPriority? priority,
        [FromQuery] string? search)
    {
        var result = await _mediator.Send(new GetShipmentsQuery(status, priority, search));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentDetailDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetShipmentByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("tracking/{trackingNumber}")]
    [ProducesResponseType(typeof(ShipmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentDetailDto>> GetByTrackingNumber(string trackingNumber)
    {
        var result = await _mediator.Send(new GetShipmentByTrackingNumberQuery(trackingNumber));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ShipmentDto>> Create([FromBody] CreateShipmentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ShipmentDto>> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request)
    {
        var command = new UpdateShipmentStatusCommand(id, request.NewStatus, request.Location, request.Reason);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record UpdateShipmentStatusRequest(
    ShipmentStatus NewStatus,
    string Location,
    string Reason
);
