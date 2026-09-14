using GestionOpe.Application.DTOs;
using GestionOpe.Application.Features.Dashboard;
using GestionOpe.Application.Features.Incidents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOpe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IncidentDto>>> GetAll(
        [FromQuery] Guid? shipmentId,
        [FromQuery] bool onlyOpen = false)
    {
        var result = await _mediator.Send(new GetIncidentsQuery(shipmentId, onlyOpen));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IncidentDto>> Create([FromBody] CreateIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        return Created($"/api/incidents/{result.Id}", result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardMetricsDto>> GetMetrics()
    {
        var result = await _mediator.Send(new GetDashboardMetricsQuery());
        return Ok(result);
    }
}
