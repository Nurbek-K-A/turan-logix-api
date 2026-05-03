using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string email, UserRole role);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}

public interface ISignatureService
{
    Task<string> SignDocumentAsync(string fileHash, string certificate, CancellationToken cancellationToken = default);
    Task<bool> VerifySignatureAsync(string fileHash, string signatureData, CancellationToken cancellationToken = default);
}

public interface IAiChatService
{
    Task<string> SendMessageAsync(int sessionId, string userMessage, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendAsync(string recipient, string message, NotificationChannel channel, CancellationToken cancellationToken = default);
}

public interface IMapboxService
{
    Task<(double Lat, double Lng)?> GeocodeAsync(string city, CancellationToken cancellationToken = default);
    double CalculateDistance(double lat1, double lng1, double lat2, double lng2);
}
