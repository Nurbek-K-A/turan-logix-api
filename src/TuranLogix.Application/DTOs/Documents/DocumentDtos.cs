using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.DTOs.Documents;

/// <summary>
/// Краткое представление документа к заявке
/// </summary>
public record DocumentDto(
    [SwaggerSchema("Id документа")] int Id,
    [SwaggerSchema("Название документа")] string Title,
    [SwaggerSchema("Тип документа")] DocumentType Type,
    [SwaggerSchema("URL файла в Azure Blob Storage")] string FileUrl,
    [SwaggerSchema("SHA-256 хэш файла")] string? FileHash,
    [SwaggerSchema("Id заявки")] int OrderId,
    [SwaggerSchema("Id пользователя, загрузившего документ")] int UploadedByUserId,
    [SwaggerSchema("Признак наличия ЭЦП-подписи")] bool IsSigned,
    [SwaggerSchema("Дата и время подписания (UTC)")] DateTime? SignedAt,
    [SwaggerSchema("Дата создания записи (UTC)")] DateTime CreatedAt);
