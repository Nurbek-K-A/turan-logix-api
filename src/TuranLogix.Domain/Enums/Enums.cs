namespace TuranLogix.Domain.Enums;

public enum UserRole
{
    Client = 1,
    Manager = 2,
    Admin = 3
}

public enum OrderStatus
{
    New = 1,
    InReview = 2,
    Confirmed = 3,
    InTransit = 4,
    Delivered = 5,
    Cancelled = 6
}

public enum CargoType
{
    General = 1,
    Fragile = 2,
    Perishable = 3,
    Dangerous = 4,
    Oversized = 5,
    Liquid = 6
}

public enum DocumentType
{
    Contract = 1,
    OrderApplication = 2,
    Waybill = 3,
    Invoice = 4,
    PackingList = 5,
    Other = 6
}

public enum MessageRole
{
    User = 1,
    Assistant = 2,
    System = 3
}

public enum NotificationChannel
{
    Telegram = 1,
    Email = 2,
    WhatsApp = 3
}
