using FluentValidation;
using TuranLogix.Application.Features.Auth.Commands;

namespace TuranLogix.Application.Features.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("ФИО обязательно")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Некорректный формат номера телефона");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов");

        RuleFor(x => x.Bin)
            .Length(12).WithMessage("БИН должен содержать ровно 12 символов")
            .When(x => x.Bin is not null);
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}
