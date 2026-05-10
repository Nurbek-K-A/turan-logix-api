using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ITelegramBotClient botClient,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _botClient = botClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string recipient, string message, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        switch (channel)
        {
            case NotificationChannel.Telegram:
                await SendTelegramAsync(recipient, message, cancellationToken);
                break;
            case NotificationChannel.Email:
                _logger.LogInformation("Email уведомление: {Recipient} — {Message}", recipient, message);
                break;
            case NotificationChannel.WhatsApp:
                _logger.LogInformation("WhatsApp уведомление (stub): {Recipient} — {Message}", recipient, message);
                break;
        }
    }

    private async Task SendTelegramAsync(string chatId, string message, CancellationToken cancellationToken)
    {
        try
        {
            var targetChatId = string.IsNullOrEmpty(chatId)
                ? _configuration["Telegram:ManagerChatId"] ?? string.Empty
                : chatId;

            if (string.IsNullOrEmpty(targetChatId))
            {
                _logger.LogWarning("Telegram ChatId не задан, уведомление не отправлено");
                return;
            }

            await _botClient.SendMessage(
                chatId: targetChatId,
                text: message,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке Telegram уведомления в чат {ChatId}", chatId);
        }
    }
}
