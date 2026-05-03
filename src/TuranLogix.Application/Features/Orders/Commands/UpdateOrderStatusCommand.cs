using MediatR;
using TuranLogix.Application.Common.Models;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Orders.Commands;

public record UpdateOrderStatusCommand(int OrderId, OrderStatus Status, decimal? Price) : IRequest<Result>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(DomainErrors.Order.NotFound);

        order.UpdateStatus(request.Status, request.Price);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
