using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.Common.Interfaces;

/// <summary>
/// Сервис генерации JWT-токенов
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Сгенерировать подписанный JWT-токен
    /// </summary>
    /// <param name="userId">Id пользователя</param>
    /// <param name="email">Email пользователя</param>
    /// <param name="role">Роль пользователя</param>
    /// <returns>Строка JWT-токена</returns>
    string GenerateToken(int userId, string email, UserRole role);
}

/// <summary>
/// Сервис хэширования и проверки паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хэшировать пароль с помощью BCrypt
    /// </summary>
    /// <param name="password">Открытый пароль</param>
    /// <returns>BCrypt-хэш</returns>
    string Hash(string password);

    /// <summary>
    /// Проверить соответствие пароля хэшу
    /// </summary>
    /// <param name="password">Открытый пароль</param>
    /// <param name="hash">Сохранённый BCrypt-хэш</param>
    /// <returns>true, если пароль корректен</returns>
    bool Verify(string password, string hash);
}

/// <summary>
/// Сервис получения данных текущего аутентифицированного пользователя из HTTP-контекста
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Id аутентифицированного пользователя (null для анонимных)</summary>
    int? UserId { get; }

    /// <summary>Email аутентифицированного пользователя</summary>
    string? Email { get; }

    /// <summary>Роль аутентифицированного пользователя</summary>
    UserRole? Role { get; }

    /// <summary>Признак аутентификации пользователя</summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// Сервис хранения файлов в облачном хранилище
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Загрузить файл в хранилище
    /// </summary>
    /// <param name="fileStream">Поток файла</param>
    /// <param name="fileName">Исходное имя файла</param>
    /// <param name="contentType">MIME-тип</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Публичный URL загруженного файла</returns>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл из хранилища
    /// </summary>
    /// <param name="fileUrl">URL ранее загруженного файла</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис ЭЦП-подписи документов через NCALayer (НУЦ РК)
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// Подписать документ сертификатом ЭЦП
    /// </summary>
    /// <param name="fileHash">SHA-256 хэш файла</param>
    /// <param name="certificate">Base64-сертификат ЭЦП</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>CMS/PKCS#7 данные подписи в Base64</returns>
    Task<string> SignDocumentAsync(string fileHash, string certificate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Верифицировать подпись документа
    /// </summary>
    /// <param name="fileHash">SHA-256 хэш файла</param>
    /// <param name="signatureData">CMS/PKCS#7 данные подписи в Base64</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true, если подпись действительна</returns>
    Task<bool> VerifySignatureAsync(string fileHash, string signatureData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис AI-чата на базе Claude (Anthropic)
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Отправить сообщение в чат и получить ответ AI
    /// </summary>
    /// <param name="sessionId">Id сессии для поддержания контекста диалога</param>
    /// <param name="userMessage">Сообщение пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Текстовый ответ AI-ассистента</returns>
    Task<string> SendMessageAsync(int sessionId, string userMessage, CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис отправки уведомлений через различные каналы
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Отправить уведомление получателю
    /// </summary>
    /// <param name="recipient">Идентификатор получателя (chat_id, email и т.д.)</param>
    /// <param name="message">Текст уведомления</param>
    /// <param name="channel">Канал доставки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SendAsync(string recipient, string message, NotificationChannel channel, CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис геокодирования и расчёта расстояний через Mapbox API
/// </summary>
public interface IMapboxService
{
    /// <summary>
    /// Получить координаты города по названию
    /// </summary>
    /// <param name="city">Название города</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Широта и долгота или null, если город не найден</returns>
    Task<(double Lat, double Lng)?> GeocodeAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Рассчитать расстояние между двумя точками по формуле Haversine
    /// </summary>
    /// <param name="lat1">Широта первой точки</param>
    /// <param name="lng1">Долгота первой точки</param>
    /// <param name="lat2">Широта второй точки</param>
    /// <param name="lng2">Долгота второй точки</param>
    /// <returns>Расстояние в километрах</returns>
    double CalculateDistance(double lat1, double lng1, double lat2, double lng2);
}
