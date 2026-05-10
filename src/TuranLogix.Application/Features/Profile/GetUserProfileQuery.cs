using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Profile;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Profile;

/// <summary>
/// Запрос профиля текущего аутентифицированного пользователя
/// </summary>
public record GetUserProfileQuery : IRequest<Result<UserProfileDto>>;

/// <summary>
/// Обработчик запроса <see cref="GetUserProfileQuery"/>
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    public GetUserProfileQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <param name="request">Пустой запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Профиль пользователя или ошибка User.NotFound</returns>
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure<UserProfileDto>(DomainErrors.User.NotFound);

        var dto = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.CompanyName,
            user.Bin,
            user.IsVerified,
            user.Role.ToString());

        return Result.Success(dto);
    }
}
