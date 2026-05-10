namespace TuranLogix.Domain.Common;

/// <summary>
/// Базовый класс для всех доменных сущностей
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Первичный ключ сущности
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Дата и время создания записи (UTC)
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата и время последнего обновления (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Доменные события, поднятые сущностью и ещё не опубликованные
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Добавить доменное событие в очередь публикации
    /// </summary>
    /// <param name="domainEvent">Доменное событие</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Очистить список поднятых доменных событий после их публикации
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Установить <see cref="UpdatedAt"/> в текущее UTC-время
    /// </summary>
    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
