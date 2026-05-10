using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuranLogix.Infrastructure.Options;

namespace TuranLogix.Infrastructure.Services.Notifications;

/// <summary>
/// Отправляет SMS-уведомления через Bird (MessageBird) API
/// </summary>
public class SmsSender
{
    private const string Originator = "TuranLogix";

    private readonly IMessageBirdAdapter _adapter;
    private readonly BirdOptions _options;
    private readonly ILogger<SmsSender> _logger;

    /// <param name="adapter">Адаптер MessageBird SDK</param>
    /// <param name="options">Параметры Bird</param>
    /// <param name="logger">Логгер</param>
    public SmsSender(IMessageBirdAdapter adapter, IOptions<BirdOptions> options, ILogger<SmsSender> logger)
    {
        _adapter = adapter;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Отправить SMS на указанный номер телефона
    /// </summary>
    /// <param name="phoneNumber">Номер получателя в формате E.164 или MSISDN (например, +77001234567 или 77001234567)</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="Exception">Пробрасывает исключение SDK при ошибке отправки после логирования</exception>
    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var normalized = phoneNumber.TrimStart('+');
        if (!long.TryParse(normalized, out var msisdn))
        {
            _logger.LogWarning("Неверный формат номера телефона для SMS: {PhoneNumber}", phoneNumber);
            return;
        }

        try
        {
            await Task.Run(() => _adapter.SendSms(Originator, new[] { msisdn }, message), cancellationToken);
            _logger.LogInformation("SMS отправлено на {PhoneNumber}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке SMS на {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}
