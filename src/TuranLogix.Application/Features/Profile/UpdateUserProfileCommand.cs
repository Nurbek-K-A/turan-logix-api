using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
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
    string? Bin) : IRequest<Result>;

/// <summary>
/// Обработчик команды <see cref="UpdateUserProfileCommand"/>
/// </summary>
public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Обновить редактируемые поля профиля и сохранить в БД
    /// </summary>
    /// <param name="request">Новые данные профиля</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Успех или ошибка User.NotFound</returns>
    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(DomainErrors.User.NotFound);

        user.Update(request.FullName, request.PhoneNumber, request.CompanyName, request.Bin);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
