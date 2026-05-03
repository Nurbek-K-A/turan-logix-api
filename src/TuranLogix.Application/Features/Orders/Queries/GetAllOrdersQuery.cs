using AutoMapper;
using MediatR;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Queries;

public record GetAllOrdersQuery(OrderStatus? Status = null) : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        if (request.Status.HasValue)
            orders = orders.Where(o => o.Status == request.Status.Value).ToList().AsReadOnly();

        var dtos = _mapper.Map<IReadOnlyList<OrderSummaryDto>>(orders);
        return Result.Success(dtos);
    }
}
