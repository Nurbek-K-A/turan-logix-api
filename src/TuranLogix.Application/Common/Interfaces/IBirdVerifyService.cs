namespace TuranLogix.Application.Common.Interfaces;

/// <summary>
/// Сервис OTP-верификации номера телефона через Bird (MessageBird) Verify API
/// </summary>
public interface IBirdVerifyService
{
    /// <summary>Время жизни OTP-кода в секундах (из конфигурации)</summary>
    int TimeoutSeconds { get; }

    /// <summary>
    /// Инициировать отправку OTP-кода на номер телефона
    /// </summary>
    /// <param name="phoneNumber">Номер телефона в формате E.164 (например, +77001234567)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Идентификатор верификации (verifyId), необходимый для подтверждения кода</returns>
    Task<string> SendOtpAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Проверить OTP-код пользователя
    /// </summary>
    /// <param name="verifyId">Идентификатор верификации, полученный от <see cref="SendOtpAsync"/></param>
    /// <param name="token">Введённый пользователем OTP-код</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true, если код корректен; false — если неверен или истёк</returns>
    Task<bool> VerifyOtpAsync(string verifyId, string token, CancellationToken ct = default);
}
