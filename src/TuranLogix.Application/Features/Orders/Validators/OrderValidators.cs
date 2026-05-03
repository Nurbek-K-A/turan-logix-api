using FluentValidation;
using TuranLogix.Application.Features.Orders.Commands;

namespace TuranLogix.Application.Features.Orders.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OriginCity)
            .NotEmpty().WithMessage("Город отправления обязателен")
            .MaximumLength(100);

        RuleFor(x => x.DestinationCity)
            .NotEmpty().WithMessage("Город назначения обязателен")
            .MaximumLength(100);

        RuleFor(x => x.CargoDescription)
            .NotEmpty().WithMessage("Описание груза обязательно")
            .MaximumLength(500);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Вес должен быть больше нуля");

        RuleFor(x => x.Volume)
            .GreaterThan(0).WithMessage("Объём должен быть больше нуля");

        RuleFor(x => x.PickupDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Дата отгрузки не может быть в прошлом");
    }
}

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Некорректный идентификатор заказа");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Цена должна быть больше нуля")
            .When(x => x.Price.HasValue);
    }
}
