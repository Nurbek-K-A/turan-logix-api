using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

public sealed class Document : BaseEntity
{
    private Document() { }

    public string Title { get; private set; } = string.Empty;
    public DocumentType Type { get; private set; }
    public string FileUrl { get; private set; } = string.Empty;
    public string? FileHash { get; private set; }
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public int UploadedByUserId { get; private set; }
    public bool IsSigned { get; private set; }
    public string? SignatureData { get; private set; }
    public DateTime? SignedAt { get; private set; }

    public static Document Create(
        string title,
        DocumentType type,
        string fileUrl,
        string? fileHash,
        int orderId,
        int uploadedByUserId)
    {
        return new Document
        {
            Title = title,
            Type = type,
            FileUrl = fileUrl,
            FileHash = fileHash,
            OrderId = orderId,
            UploadedByUserId = uploadedByUserId,
            IsSigned = false
        };
    }

    public void Sign(string signatureData)
    {
        IsSigned = true;
        SignatureData = signatureData;
        SignedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
