using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Events;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.EventHandlers;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(INotificationService notificationService, ILogger<OrderCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Создан новый заказ {OrderNumber} от клиента {ClientId}",
            notification.OrderNumber, notification.ClientId);

        await _notificationService.SendAsync(
            string.Empty,
            $"Новый заказ {notification.OrderNumber} от клиента #{notification.ClientId}",
            NotificationChannel.Telegram,
            cancellationToken);
    }
}

public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Статус заказа {OrderId} изменён с {OldStatus} на {NewStatus}",
            notification.OrderId, notification.OldStatus, notification.NewStatus);
    }
}
