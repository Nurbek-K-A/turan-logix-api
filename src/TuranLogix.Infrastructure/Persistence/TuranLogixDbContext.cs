using Microsoft.EntityFrameworkCore;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Infrastructure.Persistence;

public class TuranLogixDbContext : DbContext, IUnitOfWork
{
    public TuranLogixDbContext(DbContextOptions<TuranLogixDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuranLogixDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
