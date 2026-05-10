namespace TuranLogix.Domain.Common;

/// <summary>
/// Маркерный интерфейс доменного события
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Дата и время возникновения события (UTC)
    /// </summary>
    DateTime OccurredAt { get; }
}
