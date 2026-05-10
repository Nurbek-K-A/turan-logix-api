using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Phone.Commands;

/// <summary>
/// Команда отправки OTP-кода на номер телефона текущего пользователя.
/// Номер берётся из профиля аутентифицированного пользователя — клиент не передаёт его явно.
/// </summary>
/// <param name="UserId">Id аутентифицированного пользователя (из JWT)</param>
public record SendPhoneOtpCommand(int UserId) : IRequest<Result<string>>;

/// <summary>
/// Обработчик команды <see cref="SendPhoneOtpCommand"/>
/// </summary>
public class SendPhoneOtpCommandHandler : IRequestHandler<SendPhoneOtpCommand, Result<string>>
{
    private readonly IBirdVerifyService _verifyService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendPhoneOtpCommandHandler> _logger;

    public SendPhoneOtpCommandHandler(
        IBirdVerifyService verifyService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendPhoneOtpCommandHandler> logger)
    {
        _verifyService = verifyService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SendPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<string>(DomainErrors.User.NotFound);

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            _logger.LogWarning("Пользователь {UserId} попытался отправить OTP без номера телефона в профиле", request.UserId);
            return Result.Failure<string>(DomainErrors.Phone.PhoneNotSet);
        }

        try
        {
            var verifyId = await _verifyService.SendOtpAsync(user.PhoneNumber, cancellationToken);
            user.SetPendingVerification(verifyId, user.PhoneNumber, _verifyService.TimeoutSeconds);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OTP отправлен пользователю {UserId} на {PhoneNumber}, verifyId: {VerifyId}",
                request.UserId, user.PhoneNumber, verifyId);

            return Result.Success(verifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить OTP пользователю {UserId} на {PhoneNumber}",
                request.UserId, user.PhoneNumber);
            return Result.Failure<string>(DomainErrors.Phone.OtpSendFailed);
        }
    }
}

/// <summary>
/// Команда подтверждения OTP-кода и верификации номера телефона пользователя
/// </summary>
/// <param name="VerifyId">Идентификатор верификации, полученный от <see cref="SendPhoneOtpCommand"/></param>
/// <param name="Token">OTP-код из SMS</param>
/// <param name="UserId">Id текущего пользователя (из JWT)</param>
public record ConfirmPhoneOtpCommand(string VerifyId, string Token, int UserId) : IRequest<Result<bool>>;

/// <summary>
/// Обработчик команды <see cref="ConfirmPhoneOtpCommand"/>
/// </summary>
public class ConfirmPhoneOtpCommandHandler : IRequestHandler<ConfirmPhoneOtpCommand, Result<bool>>
{
    private readonly IBirdVerifyService _verifyService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmPhoneOtpCommandHandler> _logger;

    public ConfirmPhoneOtpCommandHandler(
        IBirdVerifyService verifyService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmPhoneOtpCommandHandler> logger)
    {
        _verifyService = verifyService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ConfirmPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<bool>(DomainErrors.User.NotFound);

        if (user.PendingVerifyId != request.VerifyId)
        {
            _logger.LogWarning("Пользователь {UserId}: verifyId не совпадает (ожидался {Expected}, получен {Got})",
                request.UserId, user.PendingVerifyId, request.VerifyId);
            return Result.Failure<bool>(DomainErrors.Phone.InvalidVerifySession);
        }

        if (user.PendingVerifyPhone != user.PhoneNumber)
        {
            _logger.LogWarning("Пользователь {UserId}: номер телефона изменён после отправки OTP", request.UserId);
            return Result.Failure<bool>(DomainErrors.Phone.PhoneChangedAfterOtp);
        }

        if (user.PendingVerifyExpiresAt is null || user.PendingVerifyExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Пользователь {UserId}: OTP-сессия истекла (expired at {ExpiresAt})",
                request.UserId, user.PendingVerifyExpiresAt);
            return Result.Failure<bool>(DomainErrors.Phone.OtpSessionExpired);
        }

        var isValid = await _verifyService.VerifyOtpAsync(request.VerifyId, request.Token, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning("Пользователь {UserId}: неверный OTP-код для verifyId {VerifyId}",
                request.UserId, request.VerifyId);
            return Result.Failure<bool>(DomainErrors.Phone.OtpVerificationFailed);
        }

        user.MarkPhoneVerified();
        user.ClearPendingVerification();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Телефон пользователя {UserId} успешно верифицирован", request.UserId);
        return Result.Success(true);
    }
}
