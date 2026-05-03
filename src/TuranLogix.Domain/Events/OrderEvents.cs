using MediatR;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Events;

public record OrderCreatedEvent(string OrderNumber, int ClientId) : INotification;

public record OrderStatusChangedEvent(int OrderId, OrderStatus OldStatus, OrderStatus NewStatus) : INotification;
