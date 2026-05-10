using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

/// <summary>
/// Сообщение в чате с AI-ассистентом
/// </summary>
public sealed class ChatMessage : BaseEntity
{
    private ChatMessage() { }

    /// <summary>Id сессии диалога</summary>
    public int SessionId { get; private set; }

    /// <summary>Id пользователя (null для системных и анонимных сообщений)</summary>
    public int? UserId { get; private set; }

    /// <summary>Роль отправителя сообщения</summary>
    public MessageRole Role { get; private set; }

    /// <summary>Текст сообщения</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Создать новое сообщение чата
    /// </summary>
    /// <param name="sessionId">Id сессии</param>
    /// <param name="role">Роль отправителя</param>
    /// <param name="content">Текст сообщения</param>
    /// <param name="userId">Id пользователя (опционально)</param>
    /// <returns>Новая сущность сообщения</returns>
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
