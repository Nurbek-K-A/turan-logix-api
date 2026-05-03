using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

public sealed class User : BaseEntity
{
    private readonly List<Order> _orders = new();

    private User() { }

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Bin { get; private set; }
    public bool IsVerified { get; private set; }
    public string? TelegramChatId { get; private set; }
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public static User Create(
        string fullName,
        string email,
        string phoneNumber,
        string passwordHash,
        UserRole role = UserRole.Client,
        string? companyName = null,
        string? bin = null)
    {
        return new User
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Role = role,
            CompanyName = companyName,
            Bin = bin,
            IsVerified = false
        };
    }

    public void Verify() => IsVerified = true;

    public void SetTelegramChatId(string chatId)
    {
        TelegramChatId = chatId;
        SetUpdatedAt();
    }

    public void Update(string fullName, string phoneNumber, string? companyName, string? bin)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        CompanyName = companyName;
        Bin = bin;
        SetUpdatedAt();
    }
}
