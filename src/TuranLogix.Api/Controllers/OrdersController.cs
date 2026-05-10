using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Application.Features.Orders.Commands;
using TuranLogix.Application.Features.Orders.Queries;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Управление заявками на грузоперевозку
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public OrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Получить список заявок текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список заявок клиента</returns>
    [HttpGet("my")]
    [SwaggerOperation(Summary = "Мои заявки", Description = "Возвращает все заявки аутентифицированного клиента")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyOrdersQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Получить детальную информацию о заявке по Id
    /// </summary>
    /// <param name="id">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Детальное представление заявки</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Заявка по Id", Description = "Клиент видит только свои заявки. Manager/Admin — любые")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (result.IsFailure)
            return result.Error.Code.Contains("AccessDenied") ? Forbid() : NotFound(new { error = result.Error.Message });
        return Ok(result.Value);
    }

    /// <summary>
    /// Создать новую заявку на перевозку
    /// </summary>
    /// <param name="request">Данные заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id созданной заявки</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Создать заявку", Description = "Создаёт заявку для текущего клиента. Геокодирует города через Mapbox")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Изменить статус заявки
    /// </summary>
    /// <param name="id">Id заявки</param>
    /// <param name="request">Новый статус и цена</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Manager,Admin")]
    [SwaggerOperation(Summary = "Обновить статус заявки", Description = "Доступно Manager и Admin. Публикует событие смены статуса")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status, request.Price);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });

        return NoContent();
    }
}
