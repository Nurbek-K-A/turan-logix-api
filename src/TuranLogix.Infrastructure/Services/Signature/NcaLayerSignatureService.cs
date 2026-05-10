using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;

namespace TuranLogix.Infrastructure.Services.Signature;

// TODO: Для production необходима интеграция с НУЦ РК (https://pki.gov.kz)
// и реальная реализация через NCALayer с сертификатами ЭЦП

/// <summary>
/// Заглушка сервиса ЭЦП-подписи через NCALayer (НУЦ РК)
/// </summary>
public class NcaLayerSignatureService : ISignatureService
{
    private readonly ILogger<NcaLayerSignatureService> _logger;

    /// <param name="logger">Логгер</param>
    public NcaLayerSignatureService(ILogger<NcaLayerSignatureService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <remarks>Stub-реализация: возвращает Base64-строку без реальной подписи</remarks>
    public Task<string> SignDocumentAsync(string fileHash, string certificate, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NCALayer подпись не реализована (stub). FileHash: {FileHash}", fileHash);
        // stub: возвращает base64 заглушку
        var stubSignature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"STUB_CMS_PKCS7:{fileHash}:{DateTime.UtcNow:O}"));
        return Task.FromResult(stubSignature);
    }

    /// <inheritdoc/>
    /// <remarks>Stub-реализация: всегда возвращает true</remarks>
    public Task<bool> VerifySignatureAsync(string fileHash, string signatureData, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NCALayer верификация подписи не реализована (stub). FileHash: {FileHash}", fileHash);
        return Task.FromResult(true);
    }
}
