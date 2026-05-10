using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuranLogix.Infrastructure.Options;

namespace TuranLogix.Infrastructure.Services.Notifications;

/// <summary>
/// Отправляет WhatsApp-уведомления через Bird (MessageBird) Conversations API.
/// Использует предварительно одобренный шаблон сообщения.
/// </summary>
public class WhatsAppSender
{
    private const string SendEndpoint = "/v1/send";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BirdOptions _options;
    private readonly ILogger<WhatsAppSender> _logger;

    /// <param name="httpClientFactory">Фабрика HTTP-клиентов (именованный клиент "Bird")</param>
    /// <param name="options">Параметры Bird</param>
    /// <param name="logger">Логгер</param>
    public WhatsAppSender(IHttpClientFactory httpClientFactory, IOptions<BirdOptions> options, ILogger<WhatsAppSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Отправить WhatsApp-сообщение на указанный номер через шаблон Bird
    /// </summary>
    /// <param name="phoneNumber">Номер получателя в формате E.164 (например, +77001234567)</param>
    /// <param name="message">Текст, подставляемый в первый параметр шаблона</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="HttpRequestException">При неуспешном HTTP-ответе от Bird API</exception>
    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.WhatsApp.ChannelId))
        {
            _logger.LogWarning("WhatsApp ChannelId не настроен — сообщение не отправлено");
            return;
        }

        var payload = new
        {
            to = phoneNumber,
            from = _options.WhatsApp.ChannelId,
            type = "hsm",
            content = new
            {
                hsm = new
                {
                    @namespace = _options.WhatsApp.Namespace,
                    templateName = _options.WhatsApp.TemplateName,
                    language = new
                    {
                        policy = "deterministic",
                        code = "ru"
                    },
                    @params = new[]
                    {
                        new { @default = message }
                    }
                }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("Bird");
            var response = await client.PostAsJsonAsync(SendEndpoint, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("WhatsApp сообщение отправлено на {PhoneNumber}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке WhatsApp сообщения на {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}
