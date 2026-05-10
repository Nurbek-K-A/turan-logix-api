using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Profile;
using TuranLogix.Application.Features.Profile;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Управление профилем текущего пользователя
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public ProfileController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные профиля</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Получить профиль", Description = "Возвращает профиль аутентифицированного пользователя")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Обновить профиль текущего пользователя
    /// </summary>
    /// <param name="command">Новые данные профиля</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpPut]
    [SwaggerOperation(Summary = "Обновить профиль", Description = "Обновляет FullName, PhoneNumber, CompanyName и BIN пользователя")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
