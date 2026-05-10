namespace TuranLogix.Domain.Enums;

/// <summary>
/// Роль пользователя в системе
/// </summary>
public enum UserRole
{
    /// <summary>Клиент — создаёт заявки</summary>
    Client = 1,

    /// <summary>Менеджер — обрабатывает заявки</summary>
    Manager = 2,

    /// <summary>Администратор — полный доступ</summary>
    Admin = 3
}

/// <summary>
/// Статус заявки на перевозку
/// </summary>
public enum OrderStatus
{
    /// <summary>Новая заявка, ожидает рассмотрения</summary>
    New = 1,

    /// <summary>Заявка на рассмотрении у менеджера</summary>
    InReview = 2,

    /// <summary>Заявка подтверждена</summary>
    Confirmed = 3,

    /// <summary>Груз в пути</summary>
    InTransit = 4,

    /// <summary>Груз доставлен</summary>
    Delivered = 5,

    /// <summary>Заявка отменена</summary>
    Cancelled = 6
}

/// <summary>
/// Тип перевозимого груза
/// </summary>
public enum CargoType
{
    /// <summary>Обычный груз</summary>
    General = 1,

    /// <summary>Хрупкий груз</summary>
    Fragile = 2,

    /// <summary>Скоропортящийся груз</summary>
    Perishable = 3,

    /// <summary>Опасный груз</summary>
    Dangerous = 4,

    /// <summary>Негабаритный груз</summary>
    Oversized = 5,

    /// <summary>Жидкий груз</summary>
    Liquid = 6
}

/// <summary>
/// Тип документа к заявке
/// </summary>
public enum DocumentType
{
    /// <summary>Договор</summary>
    Contract = 1,

    /// <summary>Заявка на перевозку</summary>
    OrderApplication = 2,

    /// <summary>Накладная</summary>
    Waybill = 3,

    /// <summary>Инвойс</summary>
    Invoice = 4,

    /// <summary>Упаковочный лист</summary>
    PackingList = 5,

    /// <summary>Прочий документ</summary>
    Other = 6
}

/// <summary>
/// Роль участника диалога в чате
/// </summary>
public enum MessageRole
{
    /// <summary>Сообщение от пользователя</summary>
    User = 1,

    /// <summary>Ответ AI-ассистента</summary>
    Assistant = 2,

    /// <summary>Системный промпт</summary>
    System = 3
}

/// <summary>
/// Канал доставки уведомлений
/// </summary>
public enum NotificationChannel
{
    /// <summary>Telegram Bot API</summary>
    Telegram = 1,

    /// <summary>Электронная почта</summary>
    Email = 2,

    /// <summary>WhatsApp via Bird (MessageBird) Conversations API</summary>
    WhatsApp = 3,

    /// <summary>SMS via Bird (MessageBird)</summary>
    SMS = 4
}
