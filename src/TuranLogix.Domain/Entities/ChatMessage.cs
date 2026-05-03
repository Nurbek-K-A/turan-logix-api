using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

public sealed class ChatMessage : BaseEntity
{
    private ChatMessage() { }

    public int SessionId { get; private set; }
    public int? UserId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;

    public static ChatMessage Create(int sessionId, MessageRole role, string content, int? userId = null)
    {
        return new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            UserId = userId
        };
    }
}
