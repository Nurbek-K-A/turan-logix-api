using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuranLogix.Application.Features.Orders.Queries;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Manager,Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllOrdersQuery(status), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
