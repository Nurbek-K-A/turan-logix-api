using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace TuranLogix.Application.DTOs.Chat;

/// <summary>
/// Запрос на отправку сообщения в чат с AI-ассистентом
/// </summary>
public record SendChatMessageRequest(
    [SwaggerSchema("Id сессии для продолжения диалога (null — новая сессия)")] int? SessionId,
    [Required][SwaggerSchema("Текст сообщения пользователя")] string Message);

/// <summary>
/// Ответ чата — реплика AI-ассистента
/// </summary>
public record ChatMessageResponse(
    [SwaggerSchema("Id сессии (использовать в следующих запросах)")] int SessionId,
    [SwaggerSchema("Текстовый ответ AI-ассистента")] string Reply);
