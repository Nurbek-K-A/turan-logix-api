using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Chat;

namespace TuranLogix.Application.Features.Chat.Commands;

public record SendChatMessageCommand(int? SessionId, string Message) : IRequest<Result<ChatMessageResponse>>;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Result<ChatMessageResponse>>
{
    private readonly IAiChatService _aiChatService;
    private readonly ICurrentUserService _currentUserService;

    public SendChatMessageCommandHandler(IAiChatService aiChatService, ICurrentUserService currentUserService)
    {
        _aiChatService = aiChatService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ChatMessageResponse>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId ?? Math.Abs((_currentUserService.UserId ?? 0).GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());
        var reply = await _aiChatService.SendMessageAsync(sessionId, request.Message, cancellationToken);
        return Result.Success(new ChatMessageResponse(sessionId, reply));
    }
}
