using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Chat;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Chat.Commands;

public record SendChatMessageCommand(int? SessionId, string Message) : IRequest<Result<ChatMessageResponse>>;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Result<ChatMessageResponse>>
{
    private readonly IAiChatService _aiChatService;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SendChatMessageCommandHandler(
        IAiChatService aiChatService,
        IChatMessageRepository chatMessageRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _aiChatService = aiChatService;
        _chatMessageRepository = chatMessageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ChatMessageResponse>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId ?? Math.Abs((_currentUserService.UserId ?? 0).GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        var reply = await _aiChatService.SendMessageAsync(sessionId, request.Message, cancellationToken);

        var userMsg = ChatMessage.Create(sessionId, MessageRole.User, request.Message, _currentUserService.UserId);
        var assistantMsg = ChatMessage.Create(sessionId, MessageRole.Assistant, reply);
        await _chatMessageRepository.AddRangeAsync(new[] { userMsg, assistantMsg }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ChatMessageResponse(sessionId, reply));
    }
}
