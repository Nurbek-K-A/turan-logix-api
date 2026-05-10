using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.Features.Orders.EventHandlers;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Events;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Commands;

/// <summary>
/// Команда создания новой заявки на перевозку
/// </summary>
public record CreateOrderCommand(
    string OriginCity,
    string DestinationCity,
    string CargoDescription,
    decimal Weight,
    decimal Volume,
    CargoType CargoType,
    DateTime PickupDate,
    string? Comment) : IRequest<Result<int>>;

/// <summary>
/// Обработчик команды <see cref="CreateOrderCommand"/>
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapboxService _mapboxService;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    /// <param name="orderRepository">Репозиторий заявок</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    /// <param name="mapboxService">Сервис геокодирования Mapbox</param>
    /// <param name="mediator">MediatR для публикации событий</param>
    /// <param name="logger">Логгер</param>
    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapboxService mapboxService,
        IMediator mediator,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapboxService = mapboxService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Создать заявку, обогатить координатами и сохранить в БД
    /// </summary>
    /// <param name="request">Данные заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id созданной заявки</returns>
    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var clientId = _currentUserService.UserId!.Value;
        var order = Order.Create(
            clientId,
            request.OriginCity,
            request.DestinationCity,
            request.CargoDescription,
            request.Weight,
            request.Volume,
            request.CargoType,
            request.PickupDate,
            request.Comment);

        try
        {
            var originCoords = await _mapboxService.GeocodeAsync(request.OriginCity, cancellationToken);
            var destCoords = await _mapboxService.GeocodeAsync(request.DestinationCity, cancellationToken);
            if (originCoords.HasValue && destCoords.HasValue)
                order.SetCoordinates(originCoords.Value.Lat, originCoords.Value.Lng,
                                     destCoords.Value.Lat, destCoords.Value.Lng);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Геокодинг недоступен для заявки {OrderNumber}, продолжаем без координат",
                order.OrderNumber);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvents = order.DomainEvents.ToList();
        order.ClearDomainEvents();
        foreach (var evt in domainEvents.OfType<OrderCreatedEvent>())
            await _mediator.Publish(new OrderCreatedNotification(evt), cancellationToken);

        return Result.Success(order.Id);
    }
}
