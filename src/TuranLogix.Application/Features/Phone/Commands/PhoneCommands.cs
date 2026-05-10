using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Phone.Commands;

/// <summary>
/// Команда отправки OTP-кода на номер телефона для верификации
/// </summary>
/// <param name="PhoneNumber">Номер телефона в формате E.164 (например, +77001234567)</param>
public record SendPhoneOtpCommand(string PhoneNumber) : IRequest<Result<string>>;

/// <summary>
/// Обработчик команды <see cref="SendPhoneOtpCommand"/>
/// </summary>
public class SendPhoneOtpCommandHandler : IRequestHandler<SendPhoneOtpCommand, Result<string>>
{
    private readonly IBirdVerifyService _verifyService;
    private readonly ILogger<SendPhoneOtpCommandHandler> _logger;

    /// <param name="verifyService">Сервис OTP-верификации Bird</param>
    /// <param name="logger">Логгер</param>
    public SendPhoneOtpCommandHandler(IBirdVerifyService verifyService, ILogger<SendPhoneOtpCommandHandler> logger)
    {
        _verifyService = verifyService;
        _logger = logger;
    }

    /// <summary>
    /// Отправить OTP-код и вернуть идентификатор верификации
    /// </summary>
    /// <param name="request">Данные команды</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>VerifyId или ошибка Phone.OtpSendFailed</returns>
    public async Task<Result<string>> Handle(SendPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        if (!IsValidE164(request.PhoneNumber))
            return Result.Failure<string>(DomainErrors.Phone.InvalidFormat);

        try
        {
            var verifyId = await _verifyService.SendOtpAsync(request.PhoneNumber, cancellationToken);
            return Result.Success(verifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить OTP на {PhoneNumber}", request.PhoneNumber);
            return Result.Failure<string>(DomainErrors.Phone.OtpSendFailed);
        }
    }

    private static bool IsValidE164(string phone)
        => !string.IsNullOrWhiteSpace(phone)
           && phone.StartsWith('+')
           && phone.Length is >= 8 and <= 16
           && phone[1..].All(char.IsDigit);
}

/// <summary>
/// Команда подтверждения OTP-кода и верификации номера телефона пользователя
/// </summary>
/// <param name="VerifyId">Идентификатор верификации, полученный от <see cref="SendPhoneOtpCommand"/></param>
/// <param name="Token">OTP-код из SMS</param>
/// <param name="UserId">Id текущего пользователя</param>
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

    /// <param name="verifyService">Сервис OTP-верификации Bird</param>
    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="logger">Логгер</param>
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

    /// <summary>
    /// Проверить OTP-код и при успехе пометить телефон пользователя как верифицированный
    /// </summary>
    /// <param name="request">Данные команды</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если телефон верифицирован, false — код неверен или истёк</returns>
    public async Task<Result<bool>> Handle(ConfirmPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _verifyService.VerifyOtpAsync(request.VerifyId, request.Token, cancellationToken);

        if (!isValid)
            return Result.Success(false);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<bool>(DomainErrors.User.NotFound);

        user.MarkPhoneVerified();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Телефон пользователя {UserId} успешно верифицирован", request.UserId);
        return Result.Success(true);
    }
}
