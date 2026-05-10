using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Application.Features.Orders.Queries;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Административные операции: просмотр всех заявок с фильтрацией
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Manager,Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public AdminController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Получить все заявки системы с опциональным фильтром по статусу
    /// </summary>
    /// <param name="status">Статус фильтрации (null — все заявки)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список заявок</returns>
    [HttpGet("orders")]
    [SwaggerOperation(Summary = "Получить все заявки", Description = "Доступно Manager и Admin. Поддерживает фильтр по статусу")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllOrdersQuery(status), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
