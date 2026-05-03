using Microsoft.EntityFrameworkCore;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TuranLogixDbContext _context;

    public UserRepository(TuranLogixDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Users.ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => _context.Users.Update(user);
}

public class OrderRepository : IOrderRepository
{
    private readonly TuranLogixDbContext _context;

    public OrderRepository(TuranLogixDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Order?> GetByIdWithDocumentsAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Documents)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Client)
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Client)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await _context.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order) => _context.Orders.Update(order);
}

public class DocumentRepository : IDocumentRepository
{
    private readonly TuranLogixDbContext _context;

    public DocumentRepository(TuranLogixDbContext context) => _context = context;

    public async Task<Document?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Where(d => d.OrderId == orderId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
        => await _context.Documents.AddAsync(document, cancellationToken);

    public void Update(Document document) => _context.Documents.Update(document);
}

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly TuranLogixDbContext _context;

    public ChatMessageRepository(TuranLogixDbContext context) => _context = context;

    public async Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        => await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
        => await _context.ChatMessages.AddAsync(message, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        => await _context.ChatMessages.AddRangeAsync(messages, cancellationToken);
}
