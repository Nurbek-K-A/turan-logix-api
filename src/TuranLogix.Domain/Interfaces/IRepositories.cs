using TuranLogix.Domain.Entities;

namespace TuranLogix.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithDocumentsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
}

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    void Update(Document document);
}

public interface IChatMessageRepository
{
    Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
