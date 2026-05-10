using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;

namespace TuranLogix.Infrastructure.Services.Storage;

/// <summary>
/// Хранилище файлов на базе Azure Blob Storage
/// </summary>
public class BlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    /// <param name="configuration">Конфигурация (AzureStorage:ConnectionString, AzureStorage:ContainerName)</param>
    /// <param name="logger">Логгер</param>
    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
        var containerName = configuration["AzureStorage:ContainerName"] ?? "turanlogix-docs";

        if (!string.IsNullOrEmpty(connectionString))
        {
            var serviceClient = new BlobServiceClient(connectionString);
            _containerClient = serviceClient.GetBlobContainerClient(containerName);
        }
        else
        {
            _containerClient = new BlobContainerClient(new Uri("https://placeholder.blob.core.windows.net/placeholder"), null as Azure.Core.TokenCredential);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Blob-имя формируется как yyyyMMdd/GUID_filename для удобства группировки</remarks>
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobName = $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}_{fileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке файла {FileName} в Azure Blob Storage", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobName = uri.AbsolutePath.TrimStart('/');
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении файла {FileUrl}", fileUrl);
        }
    }
}
