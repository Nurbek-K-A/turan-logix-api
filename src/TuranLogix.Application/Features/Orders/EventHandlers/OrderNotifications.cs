using MediatR;
using TuranLogix.Domain.Events;

namespace TuranLogix.Application.Features.Orders.EventHandlers;

public record OrderCreatedNotification(OrderCreatedEvent DomainEvent) : INotification;
public record OrderStatusChangedNotification(OrderStatusChangedEvent DomainEvent) : INotification;
