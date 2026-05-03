using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Auth;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Auth.Commands;

public record RegisterCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string? CompanyName,
    string? Bin) : IRequest<Result<RegisterResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

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

        return Result.Success(new RegisterResponse(user.Id));
    }
}

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

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
