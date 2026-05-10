using TuranLogix.Application.Common.Interfaces;

namespace TuranLogix.Infrastructure.Services;

/// <summary>
/// Хэширует и проверяет пароли с использованием BCrypt
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc/>
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
