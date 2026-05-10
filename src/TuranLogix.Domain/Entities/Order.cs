using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Events;

namespace TuranLogix.Domain.Entities;

/// <summary>
/// Заявка на грузоперевозку
/// </summary>
public sealed class Order : BaseEntity
{
    private readonly List<Document> _documents = new();

    private Order() { }

    /// <summary>Уникальный номер заявки в формате TL-YYYYMMDD-NNNNNN</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>Id клиента, создавшего заявку</summary>
    public int ClientId { get; private set; }

    /// <summary>Навигационное свойство клиента</summary>
    public User Client { get; private set; } = null!;

    /// <summary>Город отправления груза</summary>
    public string OriginCity { get; private set; } = string.Empty;

    /// <summary>Город назначения груза</summary>
    public string DestinationCity { get; private set; } = string.Empty;

    /// <summary>Широта точки отправления (Mapbox geocoding)</summary>
    public double? OriginLat { get; private set; }

    /// <summary>Долгота точки отправления (Mapbox geocoding)</summary>
    public double? OriginLng { get; private set; }

    /// <summary>Широта точки назначения (Mapbox geocoding)</summary>
    public double? DestinationLat { get; private set; }

    /// <summary>Долгота точки назначения (Mapbox geocoding)</summary>
    public double? DestinationLng { get; private set; }

    /// <summary>Описание груза</summary>
    public string CargoDescription { get; private set; } = string.Empty;

    /// <summary>Вес груза в килограммах</summary>
    public decimal Weight { get; private set; }

    /// <summary>Объём груза в кубических метрах</summary>
    public decimal Volume { get; private set; }

    /// <summary>Тип груза</summary>
    public CargoType CargoType { get; private set; }

    /// <summary>Текущий статус заявки</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Желаемая дата отгрузки</summary>
    public DateTime PickupDate { get; private set; }

    /// <summary>Фактическая дата доставки (заполняется при смене статуса на Delivered)</summary>
    public DateTime? DeliveryDate { get; private set; }

    /// <summary>Согласованная стоимость перевозки</summary>
    public decimal? Price { get; private set; }

    /// <summary>Дополнительный комментарий от клиента</summary>
    public string? Comment { get; private set; }

    /// <summary>Документы, прикреплённые к заявке</summary>
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    /// <summary>
    /// Создать новую заявку и поднять <see cref="OrderCreatedEvent"/>
    /// </summary>
    /// <param name="clientId">Id клиента</param>
    /// <param name="originCity">Город отправления</param>
    /// <param name="destinationCity">Город назначения</param>
    /// <param name="cargoDescription">Описание груза</param>
    /// <param name="weight">Вес в кг</param>
    /// <param name="volume">Объём в м³</param>
    /// <param name="cargoType">Тип груза</param>
    /// <param name="pickupDate">Дата отгрузки</param>
    /// <param name="comment">Дополнительный комментарий</param>
    /// <returns>Новая заявка со статусом New</returns>
    public static Order Create(
        int clientId,
        string originCity,
        string destinationCity,
        string cargoDescription,
        decimal weight,
        decimal volume,
        CargoType cargoType,
        DateTime pickupDate,
        string? comment = null)
    {
        var order = new Order
        {
            OrderNumber = $"TL-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}",
            ClientId = clientId,
            OriginCity = originCity,
            DestinationCity = destinationCity,
            CargoDescription = cargoDescription,
            Weight = weight,
            Volume = volume,
            CargoType = cargoType,
            Status = OrderStatus.New,
            PickupDate = pickupDate,
            Comment = comment
        };

        order.RaiseDomainEvent(new OrderCreatedEvent(order.OrderNumber, clientId));
        return order;
    }

    /// <summary>
    /// Задать координаты точек отправления и назначения
    /// </summary>
    /// <param name="originLat">Широта отправления</param>
    /// <param name="originLng">Долгота отправления</param>
    /// <param name="destLat">Широта назначения</param>
    /// <param name="destLng">Долгота назначения</param>
    public void SetCoordinates(
        double? originLat, double? originLng,
        double? destLat, double? destLng)
    {
        OriginLat = originLat;
        OriginLng = originLng;
        DestinationLat = destLat;
        DestinationLng = destLng;
        SetUpdatedAt();
    }

    /// <summary>
    /// Изменить статус заявки и поднять <see cref="OrderStatusChangedEvent"/>
    /// </summary>
    /// <param name="newStatus">Новый статус</param>
    /// <param name="price">Стоимость перевозки (устанавливается менеджером)</param>
    public void UpdateStatus(OrderStatus newStatus, decimal? price = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        if (price.HasValue) Price = price;
        if (newStatus == OrderStatus.Delivered) DeliveryDate = DateTime.UtcNow;
        SetUpdatedAt();

        RaiseDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus));
    }
}
