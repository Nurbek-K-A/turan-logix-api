using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.DTOs.Orders;

/// <summary>
/// Запрос на создание заявки на перевозку
/// </summary>
public record CreateOrderRequest(
    [Required][SwaggerSchema("Город отправления груза")] string OriginCity,
    [Required][SwaggerSchema("Город назначения груза")] string DestinationCity,
    [Required][SwaggerSchema("Описание груза")] string CargoDescription,
    [Required][SwaggerSchema("Вес груза в килограммах")] decimal Weight,
    [Required][SwaggerSchema("Объём груза в кубических метрах")] decimal Volume,
    [Required][SwaggerSchema("Тип груза")] CargoType CargoType,
    [Required][SwaggerSchema("Желаемая дата отгрузки")] DateTime PickupDate,
    [SwaggerSchema("Дополнительный комментарий")] string? Comment);

/// <summary>
/// Запрос на изменение статуса заявки (доступно менеджеру/администратору)
/// </summary>
public record UpdateOrderStatusRequest(
    [Required][SwaggerSchema("Новый статус заявки")] OrderStatus Status,
    [SwaggerSchema("Согласованная стоимость перевозки (необязательно)")] decimal? Price);

/// <summary>
/// Краткое представление заявки для списков
/// </summary>
public record OrderSummaryDto(
    [SwaggerSchema("Id заявки")] int Id,
    [SwaggerSchema("Уникальный номер заявки (TL-YYYYMMDD-NNNNNN)")] string OrderNumber,
    [SwaggerSchema("Город отправления")] string OriginCity,
    [SwaggerSchema("Город назначения")] string DestinationCity,
    [SwaggerSchema("Текущий статус")] OrderStatus Status,
    [SwaggerSchema("Тип груза")] CargoType CargoType,
    [SwaggerSchema("Описание груза")] string CargoDescription,
    [SwaggerSchema("Вес груза в кг")] decimal Weight,
    [SwaggerSchema("Согласованная стоимость")] decimal? Price,
    [SwaggerSchema("Дата отгрузки")] DateTime PickupDate,
    [SwaggerSchema("Дата создания заявки (UTC)")] DateTime CreatedAt);

/// <summary>
/// Детальное представление заявки с координатами и клиентом
/// </summary>
public record OrderDetailDto(
    [SwaggerSchema("Id заявки")] int Id,
    [SwaggerSchema("Уникальный номер заявки")] string OrderNumber,
    [SwaggerSchema("Id клиента")] int ClientId,
    [SwaggerSchema("Полное имя клиента")] string ClientName,
    [SwaggerSchema("Город отправления")] string OriginCity,
    [SwaggerSchema("Город назначения")] string DestinationCity,
    [SwaggerSchema("Широта точки отправления")] double? OriginLat,
    [SwaggerSchema("Долгота точки отправления")] double? OriginLng,
    [SwaggerSchema("Широта точки назначения")] double? DestinationLat,
    [SwaggerSchema("Долгота точки назначения")] double? DestinationLng,
    [SwaggerSchema("Описание груза")] string CargoDescription,
    [SwaggerSchema("Вес груза в кг")] decimal Weight,
    [SwaggerSchema("Объём груза в м³")] decimal Volume,
    [SwaggerSchema("Тип груза")] CargoType CargoType,
    [SwaggerSchema("Текущий статус")] OrderStatus Status,
    [SwaggerSchema("Дата отгрузки")] DateTime PickupDate,
    [SwaggerSchema("Фактическая дата доставки")] DateTime? DeliveryDate,
    [SwaggerSchema("Согласованная стоимость")] decimal? Price,
    [SwaggerSchema("Комментарий клиента")] string? Comment,
    [SwaggerSchema("Дата создания заявки (UTC)")] DateTime CreatedAt,
    [SwaggerSchema("Дата последнего обновления (UTC)")] DateTime? UpdatedAt);
