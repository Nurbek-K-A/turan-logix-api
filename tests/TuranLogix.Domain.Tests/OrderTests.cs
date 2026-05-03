using FluentAssertions;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Events;

namespace TuranLogix.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Create_ShouldGenerateOrderNumber_WithTLPrefix()
    {
        var order = Order.Create(1, "Алматы", "Астана", "Груз", 100m, 10m, CargoType.General, DateTime.UtcNow.AddDays(1));

        order.OrderNumber.Should().StartWith("TL-");
        order.Status.Should().Be(OrderStatus.New);
        order.ClientId.Should().Be(1);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var order = Order.Create(1, "Алматы", "Астана", "Груз", 100m, 10m, CargoType.General, DateTime.UtcNow.AddDays(1));

        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedEvent);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus_AndRaiseEvent()
    {
        var order = Order.Create(1, "Алматы", "Астана", "Груз", 100m, 10m, CargoType.General, DateTime.UtcNow.AddDays(1));
        order.ClearDomainEvents();

        order.UpdateStatus(OrderStatus.Confirmed, 50000m);

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Price.Should().Be(50000m);
        order.DomainEvents.Should().ContainSingle(e => e is OrderStatusChangedEvent);
    }

    [Fact]
    public void UpdateStatus_ToDelivered_ShouldSetDeliveryDate()
    {
        var order = Order.Create(1, "Алматы", "Астана", "Груз", 100m, 10m, CargoType.General, DateTime.UtcNow.AddDays(1));

        order.UpdateStatus(OrderStatus.Delivered);

        order.DeliveryDate.Should().NotBeNull();
        order.DeliveryDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
