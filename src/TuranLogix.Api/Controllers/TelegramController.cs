using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TuranLogix.Application.Features.Chat.Commands;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Webhook-эндпоинт для Telegram Bot API
/// </summary>
[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramController> _logger;

    /// <param name="mediator">MediatR</param>
    /// <param name="botClient">Telegram Bot клиент</param>
    /// <param name="logger">Логгер</param>
    public TelegramController(IMediator mediator, ITelegramBotClient botClient, ILogger<TelegramController> logger)
    {
        _mediator = mediator;
        _botClient = botClient;
        _logger = logger;
    }

    /// <summary>
    /// Принять входящее событие от Telegram Bot API
    /// </summary>
    /// <param name="update">Объект события Telegram</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <remarks>Обрабатывает только текстовые сообщения. Прочие типы событий игнорируются</remarks>
    [HttpPost("webhook")]
    [SwaggerOperation(Summary = "Telegram Webhook", Description = "Принимает Update от Telegram, отвечает через AI-ассистент")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram Update получен: {UpdateId}, тип: {Type}", update.Id, update.Type);

        if (update.Type != UpdateType.Message || update.Message?.Text is not { } messageText)
            return Ok();

        var chat = update.Message!.Chat;
        var sessionId = Math.Abs(chat.Id.GetHashCode());

        try
        {
            var command = new SendChatMessageCommand(sessionId, messageText);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                await _botClient.SendMessage(
                    chatId: chat.Id,
                    text: result.Value.Reply,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки Telegram сообщения от {ChatId}", chat.Id);
            await _botClient.SendMessage(
                chatId: chat.Id,
                text: "Извините, произошла ошибка. Попробуйте позже.",
                cancellationToken: cancellationToken);
        }

        return Ok();
    }
}
