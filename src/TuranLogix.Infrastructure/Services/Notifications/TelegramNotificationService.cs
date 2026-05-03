using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Telegram.Bot;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Infrastructure.Services.Notifications;

public class TelegramNotificationService : INotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        ITelegramBotClient botClient,
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger)
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
                await SendEmailAsync(recipient, message, cancellationToken);
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

    private async Task SendEmailAsync(string toEmail, string message, CancellationToken cancellationToken)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var fromEmail = _configuration["Email:From"];
        var password = _configuration["Email:Password"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogWarning("Email не настроен, уведомление пропущено");
            return;
        }

        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Уведомление TuranLogix";
            email.Body = new TextPart("plain") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(fromEmail, password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке Email на {ToEmail}", toEmail);
        }
    }
}
