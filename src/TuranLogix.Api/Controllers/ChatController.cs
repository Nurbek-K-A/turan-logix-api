using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Chat;
using TuranLogix.Application.Features.Chat.Commands;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// AI-чат ассистент TuranLogix на базе Claude (Anthropic)
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public ChatController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Отправить сообщение AI-ассистенту и получить ответ
    /// </summary>
    /// <param name="request">SessionId и текст сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id сессии и ответ AI-ассистента</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Отправить сообщение в чат", Description = "Диалог с AI-ассистентом. SessionId null создаёт новую сессию")]
    [ProducesResponseType(typeof(ChatMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendChatMessageCommand(request.SessionId, request.Message);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new { sessionId = result.Value.SessionId, reply = result.Value.Reply });
    }
}
