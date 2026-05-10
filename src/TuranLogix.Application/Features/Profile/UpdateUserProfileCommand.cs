using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Profile;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Profile;

/// <summary>
/// Команда обновления профиля текущего пользователя
/// </summary>
public record UpdateUserProfileCommand(
    string FullName,
    string PhoneNumber,
    string? CompanyName,
    string? Bin) : IRequest<Result<UpdateProfileResponse>>;

/// <summary>
/// Обработчик команды <see cref="UpdateUserProfileCommand"/>
/// </summary>
public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UpdateProfileResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpdateProfileResponse>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure<UpdateProfileResponse>(DomainErrors.User.NotFound);

        var previousPhone = user.PhoneNumber;
        user.Update(request.FullName, request.PhoneNumber, request.CompanyName, request.Bin);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var phoneVerificationRequired = previousPhone != request.PhoneNumber;
        return Result.Success(new UpdateProfileResponse(phoneVerificationRequired));
    }
}
