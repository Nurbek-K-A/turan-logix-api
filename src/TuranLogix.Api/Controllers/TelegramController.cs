using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TuranLogix.Application.Features.Chat.Commands;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(IMediator mediator, ILogger<TelegramController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получен Telegram Update: {UpdateId}", update.Id);

        if (update.Message?.Text is not { } messageText || update.Message.Chat is not { } chat)
            return Ok();

        var sessionId = Math.Abs(chat.Id.GetHashCode());

        var command = new SendChatMessageCommand(sessionId, messageText);
        await _mediator.Send(command, cancellationToken);

        return Ok();
    }
}
