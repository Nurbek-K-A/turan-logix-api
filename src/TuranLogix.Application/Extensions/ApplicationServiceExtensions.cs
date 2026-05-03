using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TuranLogix.Application.Common.Behaviors;
using TuranLogix.Application.Mappings;

namespace TuranLogix.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }
}
