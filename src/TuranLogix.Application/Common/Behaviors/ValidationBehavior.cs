using FluentValidation;
using MediatR;

namespace TuranLogix.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior — валидирует запросы через FluentValidation перед обработкой
/// </summary>
/// <typeparam name="TRequest">Тип запроса MediatR</typeparam>
/// <typeparam name="TResponse">Тип ответа MediatR</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <param name="validators">Все зарегистрированные валидаторы для типа запроса</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Выполнить все валидаторы и выбросить <see cref="ValidationException"/> при ошибках
    /// </summary>
    /// <param name="request">MediatR запрос</param>
    /// <param name="next">Следующий обработчик в pipeline</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат следующего обработчика</returns>
    /// <exception cref="ValidationException">Выбрасывается при наличии ошибок валидации</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
