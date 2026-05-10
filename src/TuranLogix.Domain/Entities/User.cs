using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

/// <summary>
/// Пользователь системы (клиент, менеджер или администратор)
/// </summary>
public sealed class User : BaseEntity
{
    private readonly List<Order> _orders = new();

    private User() { }

    /// <summary>Полное имя пользователя</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Email — уникальный идентификатор для входа</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Номер телефона</summary>
    public string PhoneNumber { get; private set; } = string.Empty;

    /// <summary>Хэш пароля (BCrypt)</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Роль пользователя</summary>
    public UserRole Role { get; private set; }

    /// <summary>Название компании (для юридических лиц)</summary>
    public string? CompanyName { get; private set; }

    /// <summary>БИН компании (для юридических лиц)</summary>
    public string? Bin { get; private set; }

    /// <summary>Признак верификации аккаунта администратором</summary>
    public bool IsVerified { get; private set; }

    /// <summary>Telegram chat_id для отправки уведомлений</summary>
    public string? TelegramChatId { get; private set; }

    /// <summary>Заявки пользователя</summary>
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    /// <param name="fullName">Полное имя</param>
    /// <param name="email">Email</param>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="passwordHash">BCrypt-хэш пароля</param>
    /// <param name="role">Роль (по умолчанию Client)</param>
    /// <param name="companyName">Название компании</param>
    /// <param name="bin">БИН компании</param>
    /// <returns>Новая сущность пользователя с IsVerified = false</returns>
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

    /// <summary>
    /// Верифицировать аккаунт пользователя
    /// </summary>
    public void Verify() => IsVerified = true;

    /// <summary>
    /// Привязать Telegram-аккаунт к профилю
    /// </summary>
    /// <param name="chatId">Telegram chat_id</param>
    public void SetTelegramChatId(string chatId)
    {
        TelegramChatId = chatId;
        SetUpdatedAt();
    }

    /// <summary>
    /// Обновить редактируемые поля профиля
    /// </summary>
    /// <param name="fullName">Полное имя</param>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="companyName">Название компании</param>
    /// <param name="bin">БИН компании</param>
    public void Update(string fullName, string phoneNumber, string? companyName, string? bin)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        CompanyName = companyName;
        Bin = bin;
        SetUpdatedAt();
    }
}
