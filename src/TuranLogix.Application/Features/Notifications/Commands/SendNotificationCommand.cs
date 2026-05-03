using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Application.Features.Notifications.Commands;

public record SendNotificationCommand(string Recipient, string Message, NotificationChannel Channel) : IRequest<Result>;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result>
{
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(request.Recipient, request.Message, request.Channel, cancellationToken);
        return Result.Success();
    }
}
