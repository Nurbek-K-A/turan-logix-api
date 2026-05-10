using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Auth;
using TuranLogix.Application.Features.Auth.Commands;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Регистрация и аутентификация пользователей
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Зарегистрировать нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id созданного пользователя</returns>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Зарегистрировать пользователя", Description = "Создаёт нового пользователя с ролью Client")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.FullName, request.Email, request.PhoneNumber,
            request.Password, request.CompanyName, request.Bin);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new { userId = result.Value.UserId });
    }

    /// <summary>
    /// Войти в систему и получить JWT-токен
    /// </summary>
    /// <param name="request">Email и пароль</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>JWT Bearer токен</returns>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Войти в систему", Description = "Возвращает JWT Bearer токен для последующих запросов")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return Unauthorized(new { error = result.Error.Message });

        return Ok(new { token = result.Value.Token });
    }
}
