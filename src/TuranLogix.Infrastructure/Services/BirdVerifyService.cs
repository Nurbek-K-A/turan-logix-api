using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Infrastructure.Options;

namespace TuranLogix.Infrastructure.Services;

/// <summary>
/// Реализует OTP-верификацию номера телефона через Bird (MessageBird) Verify API
/// </summary>
public class BirdVerifyService : IBirdVerifyService
{
    private readonly IMessageBirdAdapter _adapter;
    private readonly VerifyOptions _verifyOptions;
    private readonly ILogger<BirdVerifyService> _logger;

    /// <param name="adapter">Адаптер MessageBird SDK</param>
    /// <param name="options">Параметры Bird</param>
    /// <param name="logger">Логгер</param>
    public BirdVerifyService(IMessageBirdAdapter adapter, IOptions<BirdOptions> options, ILogger<BirdVerifyService> logger)
    {
        _adapter = adapter;
        _verifyOptions = options.Value.Verify;
        _logger = logger;
    }

    /// <summary>
    /// Инициировать OTP-верификацию и получить verifyId
    /// </summary>
    /// <param name="phoneNumber">Номер телефона в формате E.164 (например, +77001234567)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Идентификатор верификации (verifyId)</returns>
    /// <exception cref="InvalidOperationException">Если Bird API вернул ошибку</exception>
    public async Task<string> SendOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        var msisdn = phoneNumber.TrimStart('+');

        try
        {
            var verifyId = await Task.Run(
                () => _adapter.CreateVerify(msisdn, _verifyOptions.TokenLength, _verifyOptions.TimeoutSeconds),
                ct);

            _logger.LogInformation("OTP инициирован для {PhoneNumber}, verifyId: {VerifyId}", phoneNumber, verifyId);
            return verifyId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании OTP-верификации для {PhoneNumber}", phoneNumber);
            throw new InvalidOperationException("Не удалось отправить OTP-код", ex);
        }
    }

    /// <summary>
    /// Проверить OTP-код пользователя
    /// </summary>
    /// <param name="verifyId">Идентификатор верификации из <see cref="SendOtpAsync"/></param>
    /// <param name="token">OTP-код из SMS</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true — код корректен; false — неверен или истёк</returns>
    public async Task<bool> VerifyOtpAsync(string verifyId, string token, CancellationToken ct = default)
    {
        try
        {
            var result = await Task.Run(() => _adapter.VerifyToken(verifyId, token), ct);
            _logger.LogInformation("OTP-верификация {VerifyId}: {Result}", verifyId, result ? "успешна" : "неудача");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке OTP {VerifyId}", verifyId);
            return false;
        }
    }
}
