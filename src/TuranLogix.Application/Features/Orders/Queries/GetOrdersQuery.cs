using AutoMapper;
using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Queries;

public record GetMyOrdersQuery : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyOrdersQueryHandler(IOrderRepository orderRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUserService.UserId!.Value;
        var orders = await _orderRepository.GetByClientIdAsync(clientId, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<OrderSummaryDto>>(orders);
        return Result.Success(dtos);
    }
}

public record GetOrderByIdQuery(int OrderId) : IRequest<Result<OrderDetailDto>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDocumentsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderDetailDto>(DomainErrors.Order.NotFound);

        var userId = _currentUserService.UserId;
        var role = _currentUserService.Role;

        if (role == Domain.Enums.UserRole.Client && order.ClientId != userId)
            return Result.Failure<OrderDetailDto>(DomainErrors.Order.AccessDenied);

        var dto = _mapper.Map<OrderDetailDto>(order);
        return Result.Success(dto);
    }
}
