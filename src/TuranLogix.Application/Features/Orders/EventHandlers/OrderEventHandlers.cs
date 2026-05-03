using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.EventHandlers;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(INotificationService notificationService, ILogger<OrderCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Создана новая заявка {OrderNumber} от клиента {ClientId}", evt.OrderNumber, evt.ClientId);

        await _notificationService.SendAsync(
            string.Empty,
            $"Новая заявка {evt.OrderNumber} от клиента #{evt.ClientId}",
            NotificationChannel.Telegram,
            cancellationToken);
    }
}

public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedNotification>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        INotificationService notificationService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Статус заявки {OrderId}: {Old} → {New}", evt.OrderId, evt.OldStatus, evt.NewStatus);

        var order = await _orderRepository.GetByIdAsync(evt.OrderId, cancellationToken);
        if (order is null) return;

        var client = await _userRepository.GetByIdAsync(order.ClientId, cancellationToken);
        if (client?.TelegramChatId is null) return;

        var statusText = evt.NewStatus switch
        {
            OrderStatus.InReview  => "📋 принята в обработку",
            OrderStatus.Confirmed => "✅ подтверждена",
            OrderStatus.InTransit => "🚛 груз в пути",
            OrderStatus.Delivered => "🎉 доставлена! Спасибо за доверие.",
            OrderStatus.Cancelled => "❌ отменена",
            _ => evt.NewStatus.ToString()
        };

        var message = $"Заявка {order.OrderNumber} {statusText}";
        await _notificationService.SendAsync(client.TelegramChatId, message, NotificationChannel.Telegram, cancellationToken);
    }
}
