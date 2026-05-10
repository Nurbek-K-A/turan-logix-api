using TuranLogix.Domain.Entities;

namespace TuranLogix.Domain.Interfaces;

/// <summary>
/// Репозиторий пользователей
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по Id
    /// </summary>
    /// <param name="id">Id пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по email
    /// </summary>
    /// <param name="email">Email пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить нового пользователя
    /// </summary>
    /// <param name="user">Сущность пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пометить пользователя как изменённого (EF change tracking)
    /// </summary>
    /// <param name="user">Сущность пользователя</param>
    void Update(User user);
}

/// <summary>
/// Репозиторий заявок на перевозку
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Получить заявку по Id
    /// </summary>
    /// <param name="id">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить заявку по Id с загруженными документами
    /// </summary>
    /// <param name="id">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Order?> GetByIdWithDocumentsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все заявки конкретного клиента
    /// </summary>
    /// <param name="clientId">Id клиента</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<Order>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все заявки в системе
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую заявку
    /// </summary>
    /// <param name="order">Сущность заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пометить заявку как изменённую (EF change tracking)
    /// </summary>
    /// <param name="order">Сущность заявки</param>
    void Update(Order order);
}

/// <summary>
/// Репозиторий документов к заявкам
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Получить документ по Id
    /// </summary>
    /// <param name="id">Id документа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Document?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все документы заявки
    /// </summary>
    /// <param name="orderId">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<Document>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить документ
    /// </summary>
    /// <param name="document">Сущность документа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пометить документ как изменённый (EF change tracking)
    /// </summary>
    /// <param name="document">Сущность документа</param>
    void Update(Document document);
}

/// <summary>
/// Репозиторий сообщений чата
/// </summary>
public interface IChatMessageRepository
{
    /// <summary>
    /// Получить историю сообщений сессии
    /// </summary>
    /// <param name="sessionId">Id сессии чата</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить одно сообщение
    /// </summary>
    /// <param name="message">Сущность сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить пакет сообщений
    /// </summary>
    /// <param name="messages">Коллекция сообщений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddRangeAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
}

/// <summary>
/// Единица работы — фиксирует транзакцию в БД
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить все изменения в БД
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Количество затронутых строк</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
