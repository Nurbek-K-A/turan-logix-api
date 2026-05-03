using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.DTOs.Orders;

public record CreateOrderRequest(
    string OriginCity,
    string DestinationCity,
    string CargoDescription,
    decimal Weight,
    decimal Volume,
    CargoType CargoType,
    DateTime PickupDate,
    string? Comment);

public record UpdateOrderStatusRequest(OrderStatus Status, decimal? Price);

public record OrderSummaryDto(
    int Id,
    string OrderNumber,
    string OriginCity,
    string DestinationCity,
    OrderStatus Status,
    CargoType CargoType,
    decimal Weight,
    DateTime PickupDate,
    DateTime CreatedAt);

public record OrderDetailDto(
    int Id,
    string OrderNumber,
    int ClientId,
    string ClientName,
    string OriginCity,
    string DestinationCity,
    double? OriginLat,
    double? OriginLng,
    double? DestinationLat,
    double? DestinationLng,
    string CargoDescription,
    decimal Weight,
    decimal Volume,
    CargoType CargoType,
    OrderStatus Status,
    DateTime PickupDate,
    DateTime? DeliveryDate,
    decimal? Price,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
