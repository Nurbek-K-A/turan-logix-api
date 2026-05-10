using MediatR;
using Microsoft.Extensions.Logging;

namespace TuranLogix.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior — логирует входящие запросы и их завершение
/// </summary>
/// <typeparam name="TRequest">Тип запроса MediatR</typeparam>
/// <typeparam name="TResponse">Тип ответа MediatR</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <param name="logger">Логгер</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Залогировать имя запроса до и после его обработки
    /// </summary>
    /// <param name="request">MediatR запрос</param>
    /// <param name="next">Следующий обработчик в pipeline</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат следующего обработчика</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Обработка запроса {RequestName}", requestName);

        var response = await next();

        _logger.LogInformation("Запрос {RequestName} обработан успешно", requestName);
        return response;
    }
}
