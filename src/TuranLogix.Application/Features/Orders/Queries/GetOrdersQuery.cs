using AutoMapper;
using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Queries;

/// <summary>
/// Запрос заявок текущего аутентифицированного клиента
/// </summary>
public record GetMyOrdersQuery : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;

/// <summary>
/// Обработчик запроса <see cref="GetMyOrdersQuery"/>
/// </summary>
public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    /// <param name="orderRepository">Репозиторий заявок</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    /// <param name="mapper">AutoMapper</param>
    public GetMyOrdersQueryHandler(IOrderRepository orderRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    /// <summary>
    /// Вернуть заявки текущего клиента
    /// </summary>
    /// <param name="request">Пустой запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список кратких представлений заявок клиента</returns>
    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUserService.UserId!.Value;
        var orders = await _orderRepository.GetByClientIdAsync(clientId, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<OrderSummaryDto>>(orders);
        return Result.Success(dtos);
    }
}

/// <summary>
/// Запрос детальной информации о заявке по Id
/// </summary>
public record GetOrderByIdQuery(int OrderId) : IRequest<Result<OrderDetailDto>>;

/// <summary>
/// Обработчик запроса <see cref="GetOrderByIdQuery"/>
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    /// <param name="orderRepository">Репозиторий заявок</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    /// <param name="mapper">AutoMapper</param>
    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить заявку по Id с проверкой прав доступа клиента
    /// </summary>
    /// <param name="request">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Детальное представление заявки или ошибка NotFound/AccessDenied</returns>
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
