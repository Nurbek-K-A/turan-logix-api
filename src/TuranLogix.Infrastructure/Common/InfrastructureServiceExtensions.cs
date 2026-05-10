using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Interfaces;
using TuranLogix.Infrastructure.External.Mapbox;
using TuranLogix.Infrastructure.Persistence;
using TuranLogix.Infrastructure.Persistence.Repositories;
using TuranLogix.Infrastructure.Services;
using TuranLogix.Infrastructure.Services.Ai;
using TuranLogix.Infrastructure.Services.Notifications;
using TuranLogix.Infrastructure.Services.Signature;
using TuranLogix.Infrastructure.Services.Storage;

namespace TuranLogix.Infrastructure.Common;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TuranLogixDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TuranLogixDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IFileStorageService, BlobStorageService>();
        services.AddScoped<ISignatureService, NcaLayerSignatureService>();
        services.AddScoped<IAiChatService, ClaudeAiChatService>();
        services.AddScoped<INotificationService, NotificationService>();

        var botToken = configuration["Telegram:BotToken"] ?? string.Empty;
        if (!string.IsNullOrEmpty(botToken))
        {
            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        }
        else
        {
            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient("0:placeholder"));
        }

        services.AddHttpClient<IMapboxService, MapboxService>();

        return services;
    }
}
