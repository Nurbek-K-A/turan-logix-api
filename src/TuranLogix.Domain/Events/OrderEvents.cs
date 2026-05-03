using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Events;

public record OrderCreatedEvent(string OrderNumber, int ClientId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record OrderStatusChangedEvent(int OrderId, OrderStatus OldStatus, OrderStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
