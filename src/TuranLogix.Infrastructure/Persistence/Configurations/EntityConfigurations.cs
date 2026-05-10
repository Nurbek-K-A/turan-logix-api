using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuranLogix.Domain.Entities;

namespace TuranLogix.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).UseIdentityAlwaysColumn();

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
        builder.Property(u => u.CompanyName).HasMaxLength(300);
        builder.Property(u => u.Bin).HasMaxLength(12);
        builder.Property(u => u.TelegramChatId).HasMaxLength(50);
        builder.Property(u => u.IsPhoneVerified).HasDefaultValue(false);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.Orders)
            .WithOne(o => o.Client)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).UseIdentityAlwaysColumn();

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(o => o.OriginCity).IsRequired().HasMaxLength(100);
        builder.Property(o => o.DestinationCity).IsRequired().HasMaxLength(100);
        builder.Property(o => o.CargoDescription).IsRequired().HasMaxLength(500);
        builder.Property(o => o.Weight).HasPrecision(18, 3);
        builder.Property(o => o.Volume).HasPrecision(18, 3);
        builder.Property(o => o.Price).HasPrecision(18, 2);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.CargoType).HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.Comment).HasMaxLength(1000);

        builder.Ignore(o => o.DomainEvents);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.ClientId);

        builder.HasMany(o => o.Documents)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseIdentityAlwaysColumn();

        builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(1000);
        builder.Property(d => d.FileHash).HasMaxLength(64);

        builder.HasIndex(d => d.OrderId);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityAlwaysColumn();

        builder.Property(c => c.Content).IsRequired();
        builder.Property(c => c.Role).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(c => c.SessionId);
    }
}
