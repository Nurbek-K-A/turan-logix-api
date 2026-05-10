namespace TuranLogix.Infrastructure.Options;

/// <summary>
/// Параметры интеграции с Bird (MessageBird) — SMS, WhatsApp и Verify API
/// </summary>
public class BirdOptions
{
    /// <summary>Ключ доступа к Bird API</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Параметры WhatsApp канала</summary>
    public WhatsAppOptions WhatsApp { get; set; } = new();

    /// <summary>Параметры OTP-верификации</summary>
    public VerifyOptions Verify { get; set; } = new();
}

/// <summary>
/// Параметры WhatsApp канала в Bird Conversations API
/// </summary>
public class WhatsAppOptions
{
    /// <summary>Идентификатор WhatsApp канала в Bird</summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Namespace утверждённых шаблонов</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>Имя шаблона сообщения (по умолчанию "notification")</summary>
    public string TemplateName { get; set; } = "notification";
}

/// <summary>
/// Параметры Bird Verify API для OTP-верификации
/// </summary>
public class VerifyOptions
{
    /// <summary>Длина OTP-кода (по умолчанию 4 символа)</summary>
    public int TokenLength { get; set; } = 4;

    /// <summary>Время жизни OTP-кода в секундах (по умолчанию 300)</summary>
    public int TimeoutSeconds { get; set; } = 300;
}
