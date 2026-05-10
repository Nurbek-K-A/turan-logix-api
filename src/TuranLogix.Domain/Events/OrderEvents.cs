using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Events;

/// <summary>
/// Событие создания новой заявки на перевозку
/// </summary>
/// <param name="OrderNumber">Уникальный номер заявки</param>
/// <param name="ClientId">Id клиента, создавшего заявку</param>
public record OrderCreatedEvent(string OrderNumber, int ClientId) : IDomainEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Событие изменения статуса заявки
/// </summary>
/// <param name="OrderId">Id заявки</param>
/// <param name="OldStatus">Предыдущий статус</param>
/// <param name="NewStatus">Новый статус</param>
public record OrderStatusChangedEvent(int OrderId, OrderStatus OldStatus, OrderStatus NewStatus) : IDomainEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
