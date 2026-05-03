namespace TuranLogix.Application.DTOs.Chat;

public record SendChatMessageRequest(int? SessionId, string Message);

public record ChatMessageResponse(int SessionId, string Reply);
