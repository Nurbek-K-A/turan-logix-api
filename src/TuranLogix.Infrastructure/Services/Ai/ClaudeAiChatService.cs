using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Infrastructure.Services.Ai;

/// <summary>
/// AI-чат ассистент TuranLogix на базе Claude (Anthropic API)
/// </summary>
public class ClaudeAiChatService : IAiChatService
{
    private readonly AnthropicClient _client;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly ILogger<ClaudeAiChatService> _logger;

    private const string SystemPrompt = """
        Вы — интеллектуальный ассистент TuranLogix, ведущей логистической компании в Казахстане.
        Ваша задача — помогать клиентам с вопросами о грузоперевозках, отслеживании заказов,
        документации и услугах компании.

        Основные направления помощи:
        - Информация о типах грузов и условиях перевозки
        - Помощь в оформлении заказов
        - Разъяснение статусов заказов
        - Информация о необходимых документах (CMR, накладная, инвойс и т.д.)
        - Консультация по таможенному оформлению
        - Расчёт примерных сроков и стоимости доставки

        Всегда отвечайте вежливо и профессионально на языке пользователя (KZ, RU, EN, KG, UZ, ...).
        Если вопрос выходит за рамки логистики, вежливо перенаправьте пользователя к основной теме.
        """;

    /// <param name="configuration">Конфигурация (Anthropic:ApiKey)</param>
    /// <param name="chatMessageRepository">Репозиторий сообщений для передачи истории диалога</param>
    /// <param name="logger">Логгер</param>
    public ClaudeAiChatService(
        IConfiguration configuration,
        IChatMessageRepository chatMessageRepository,
        ILogger<ClaudeAiChatService> logger)
    {
        _client = new AnthropicClient(configuration["Anthropic:ApiKey"] ?? string.Empty);
        _chatMessageRepository = chatMessageRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <remarks>При ошибке API возвращает заглушку вместо выброса исключения</remarks>
    public async Task<string> SendMessageAsync(int sessionId, string userMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _chatMessageRepository.GetBySessionIdAsync(sessionId, cancellationToken);

            var messages = history
                .Where(m => m.Role != MessageRole.System)
                .Select(m => new Message
                {
                    Role = m.Role == MessageRole.User ? RoleType.User : RoleType.Assistant,
                    Content = new List<ContentBase> { new TextContent { Text = m.Content } }
                })
                .ToList();

            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = new List<ContentBase> { new TextContent { Text = userMessage } }
            });

            var parameters = new MessageParameters
            {
                Model = AnthropicModels.Claude3Haiku,
                MaxTokens = 1024,
                SystemMessage = SystemPrompt,
                Messages = messages
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters, null, cancellationToken);
            return response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обращении к Claude AI для сессии {SessionId}", sessionId);
            return "Извините, в данный момент ассистент недоступен. Попробуйте позже.";
        }
    }
}
