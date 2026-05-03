using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Commands;

public record CreateOrderCommand(
    string OriginCity,
    string DestinationCity,
    string CargoDescription,
    decimal Weight,
    decimal Volume,
    CargoType CargoType,
    DateTime PickupDate,
    string? Comment) : IRequest<Result<int>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapboxService _mapboxService;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapboxService mapboxService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapboxService = mapboxService;
    }

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

        var originCoords = await _mapboxService.GeocodeAsync(request.OriginCity, cancellationToken);
        var destCoords = await _mapboxService.GeocodeAsync(request.DestinationCity, cancellationToken);
        order.SetCoordinates(
            originCoords?.Lat, originCoords?.Lng,
            destCoords?.Lat, destCoords?.Lng);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
