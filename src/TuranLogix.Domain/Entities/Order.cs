using TuranLogix.Domain.Common;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Events;

namespace TuranLogix.Domain.Entities;

public sealed class Order : BaseEntity
{
    private readonly List<Document> _documents = new();

    private Order() { }

    public string OrderNumber { get; private set; } = string.Empty;
    public int ClientId { get; private set; }
    public User Client { get; private set; } = null!;
    public string OriginCity { get; private set; } = string.Empty;
    public string DestinationCity { get; private set; } = string.Empty;
    public double? OriginLat { get; private set; }
    public double? OriginLng { get; private set; }
    public double? DestinationLat { get; private set; }
    public double? DestinationLng { get; private set; }
    public string CargoDescription { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public decimal Volume { get; private set; }
    public CargoType CargoType { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime PickupDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public decimal? Price { get; private set; }
    public string? Comment { get; private set; }
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

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
