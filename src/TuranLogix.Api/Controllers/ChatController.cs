using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuranLogix.Application.DTOs.Chat;
using TuranLogix.Application.Features.Chat.Commands;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendChatMessageCommand(request.SessionId, request.Message);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new { sessionId = result.Value.SessionId, reply = result.Value.Reply });
    }
}
