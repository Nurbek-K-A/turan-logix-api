namespace TuranLogix.Infrastructure.Services;

/// <summary>
/// Тонкая обёртка над <c>MessageBird.Client</c>, обеспечивающая тестируемость сервисов SMS и Verify.
/// </summary>
public interface IMessageBirdAdapter
{
    /// <summary>
    /// Отправить SMS-сообщение
    /// </summary>
    /// <param name="originator">Идентификатор отправителя (имя или номер)</param>
    /// <param name="recipients">Список MSISDN получателей</param>
    /// <param name="body">Текст сообщения</param>
    void SendSms(string originator, long[] recipients, string body);

    /// <summary>
    /// Инициировать OTP-верификацию
    /// </summary>
    /// <param name="phoneNumber">Номер телефона получателя (MSISDN без +)</param>
    /// <param name="tokenLength">Длина OTP-кода</param>
    /// <param name="timeoutSeconds">Время жизни кода в секундах</param>
    /// <returns>Идентификатор верификации (verifyId)</returns>
    string CreateVerify(string phoneNumber, int tokenLength, int timeoutSeconds);

    /// <summary>
    /// Проверить OTP-код
    /// </summary>
    /// <param name="verifyId">Идентификатор верификации</param>
    /// <param name="token">OTP-код пользователя</param>
    /// <returns>true — код корректен; false — неверен или истёк</returns>
    bool VerifyToken(string verifyId, string token);
}
