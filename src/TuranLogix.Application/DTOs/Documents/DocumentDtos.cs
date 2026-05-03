using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.DTOs.Documents;

public record DocumentDto(
    int Id,
    string Title,
    DocumentType Type,
    string FileUrl,
    string? FileHash,
    int OrderId,
    int UploadedByUserId,
    bool IsSigned,
    DateTime? SignedAt,
    DateTime CreatedAt);
