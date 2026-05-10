using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Entities;

/// <summary>
/// Документ, прикреплённый к заявке на перевозку
/// </summary>
public sealed class Document : BaseEntity
{
    private Document() { }

    /// <summary>Название документа</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Тип документа</summary>
    public DocumentType Type { get; private set; }

    /// <summary>URL файла в Azure Blob Storage</summary>
    public string FileUrl { get; private set; } = string.Empty;

    /// <summary>SHA-256 хэш файла для проверки целостности</summary>
    public string? FileHash { get; private set; }

    /// <summary>Id заявки, к которой прикреплён документ</summary>
    public int OrderId { get; private set; }

    /// <summary>Навигационное свойство заявки</summary>
    public Order Order { get; private set; } = null!;

    /// <summary>Id пользователя, загрузившего документ</summary>
    public int UploadedByUserId { get; private set; }

    /// <summary>Признак наличия ЭЦП-подписи</summary>
    public bool IsSigned { get; private set; }

    /// <summary>CMS/PKCS#7 данные подписи (NCALayer)</summary>
    public string? SignatureData { get; private set; }

    /// <summary>Дата и время подписания (UTC)</summary>
    public DateTime? SignedAt { get; private set; }

    /// <summary>
    /// Создать новый документ
    /// </summary>
    /// <param name="title">Название документа</param>
    /// <param name="type">Тип документа</param>
    /// <param name="fileUrl">URL файла в хранилище</param>
    /// <param name="fileHash">SHA-256 хэш файла</param>
    /// <param name="orderId">Id заявки</param>
    /// <param name="uploadedByUserId">Id загружающего пользователя</param>
    /// <returns>Новый документ с IsSigned = false</returns>
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

    /// <summary>
    /// Подписать документ ЭЦП
    /// </summary>
    /// <param name="signatureData">CMS/PKCS#7 данные подписи от NCALayer</param>
    public void Sign(string signatureData)
    {
        IsSigned = true;
        SignatureData = signatureData;
        SignedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
