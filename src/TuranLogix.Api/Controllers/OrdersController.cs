using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Application.Features.Orders.Commands;
using TuranLogix.Application.Features.Orders.Queries;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyOrdersQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (result.IsFailure)
            return result.Error.Code.Contains("AccessDenied") ? Forbid() : NotFound(new { error = result.Error.Message });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.OriginCity, request.DestinationCity,
            request.CargoDescription, request.Weight, request.Volume,
            request.CargoType, request.PickupDate, request.Comment);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status, request.Price);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });

        return NoContent();
    }
}
