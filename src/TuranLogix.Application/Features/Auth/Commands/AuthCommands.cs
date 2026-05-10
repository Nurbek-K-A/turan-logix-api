using MediatR;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Auth;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Auth.Commands;

/// <summary>
/// Команда регистрации нового пользователя
/// </summary>
public record RegisterCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string? CompanyName,
    string? Bin) : IRequest<Result<RegisterResponse>>;

/// <summary>
/// Обработчик команды <see cref="RegisterCommand"/>
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IBirdVerifyService _verifyService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="passwordHasher">Сервис хэширования паролей</param>
    /// <param name="verifyService">Сервис OTP-верификации телефона</param>
    /// <param name="logger">Логгер</param>
    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IBirdVerifyService verifyService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _verifyService = verifyService;
        _logger = logger;
    }

    /// <summary>
    /// Зарегистрировать пользователя и инициировать OTP-верификацию телефона.
    /// Телефон помечается как неверифицированный (IsPhoneVerified = false).
    /// Если отправка OTP не удалась — регистрация не блокируется.
    /// </summary>
    /// <param name="request">Данные регистрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id созданного пользователя или ошибка EmailAlreadyExists</returns>
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
            return Result.Failure<RegisterResponse>(DomainErrors.User.EmailAlreadyExists);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.FullName, request.Email, request.PhoneNumber, passwordHash,
            companyName: request.CompanyName, bin: request.Bin);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _verifyService.SendOtpAsync(request.PhoneNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось отправить OTP на {PhoneNumber} после регистрации пользователя {UserId}. Верификация телефона отложена.",
                request.PhoneNumber, user.Id);
        }

        return Result.Success(new RegisterResponse(user.Id));
    }
}

/// <summary>
/// Команда входа пользователя в систему
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

/// <summary>
/// Обработчик команды <see cref="LoginCommand"/>
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="passwordHasher">Сервис проверки паролей</param>
    /// <param name="jwtTokenService">Сервис генерации JWT</param>
    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Аутентифицировать пользователя и вернуть JWT-токен
    /// </summary>
    /// <param name="request">Учётные данные</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>JWT-токен или ошибка InvalidCredentials</returns>
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result.Failure<LoginResponse>(DomainErrors.User.InvalidCredentials);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(DomainErrors.User.InvalidCredentials);

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.Role);
        return Result.Success(new LoginResponse(token));
    }
}
