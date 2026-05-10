using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Telegram.Bot;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Infrastructure.Services.Notifications;

/// <summary>
/// Маршрутизирует уведомления по каналам: Telegram, Email, WhatsApp, SMS
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly SmsSender _smsSender;
    private readonly WhatsAppSender _whatsAppSender;

    /// <param name="botClient">Telegram Bot клиент</param>
    /// <param name="configuration">Конфигурация (Telegram:ManagerChatId, Email:*)</param>
    /// <param name="logger">Логгер</param>
    /// <param name="smsSender">Сервис отправки SMS через Bird</param>
    /// <param name="whatsAppSender">Сервис отправки WhatsApp через Bird</param>
    public NotificationService(
        ITelegramBotClient botClient,
        IConfiguration configuration,
        ILogger<NotificationService> logger,
        SmsSender smsSender,
        WhatsAppSender whatsAppSender)
    {
        _botClient = botClient;
        _configuration = configuration;
        _logger = logger;
        _smsSender = smsSender;
        _whatsAppSender = whatsAppSender;
    }

    /// <inheritdoc/>
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
                await _whatsAppSender.SendAsync(recipient, message, cancellationToken);
                break;
            case NotificationChannel.SMS:
                await _smsSender.SendAsync(recipient, message, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Отправить сообщение в Telegram
    /// </summary>
    /// <param name="chatId">Telegram chat_id получателя (пустая строка — менеджерский чат из конфигурации)</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
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

    /// <summary>
    /// Отправить уведомление по электронной почте через SMTP
    /// </summary>
    /// <param name="toEmail">Email получателя</param>
    /// <param name="message">Текст письма</param>
    /// <param name="cancellationToken">Токен отмены</param>
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
