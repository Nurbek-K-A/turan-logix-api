using MediatR;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.Features.Orders.EventHandlers;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Events;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Commands;

/// <summary>
/// Команда изменения статуса заявки (доступна менеджеру/администратору)
/// </summary>
public record UpdateOrderStatusCommand(int OrderId, OrderStatus Status, decimal? Price) : IRequest<Result>;

/// <summary>
/// Обработчик команды <see cref="UpdateOrderStatusCommand"/>
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    /// <param name="orderRepository">Репозиторий заявок</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="mediator">MediatR для публикации событий</param>
    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    /// <summary>
    /// Изменить статус заявки и опубликовать событие <see cref="OrderStatusChangedEvent"/>
    /// </summary>
    /// <param name="request">Id заявки, новый статус и цена</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Успех или ошибка Order.NotFound</returns>
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(DomainErrors.Order.NotFound);

        order.UpdateStatus(request.Status, request.Price);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvents = order.DomainEvents.ToList();
        order.ClearDomainEvents();
        foreach (var evt in domainEvents.OfType<OrderStatusChangedEvent>())
            await _mediator.Publish(new OrderStatusChangedNotification(evt), cancellationToken);

        return Result.Success();
    }
}
