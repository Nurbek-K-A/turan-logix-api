using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.DTOs.Phone;
using TuranLogix.Application.Features.Phone.Commands;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// OTP-верификация номера телефона
/// </summary>
[ApiController]
[Route("api/phone")]
[Authorize]
public class PhoneController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    /// <param name="mediator">MediatR</param>
    /// <param name="currentUser">Сервис текущего пользователя</param>
    public PhoneController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Отправить OTP-код на номер телефона
    /// </summary>
    /// <param name="request">Номер телефона в формате E.164</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Идентификатор верификации для последующего подтверждения</returns>
    [HttpPost("send-otp")]
    [SwaggerOperation(
        Summary = "Отправить OTP-код",
        Description = "Инициирует отправку SMS с OTP-кодом. Возвращает verifyId, необходимый для confirm-otp.")]
    [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new SendPhoneOtpCommand(request.PhoneNumber);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new SendOtpResponse(result.Value));
    }

    /// <summary>
    /// Подтвердить OTP-код и верифицировать номер телефона
    /// </summary>
    /// <param name="request">VerifyId и OTP-код из SMS</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Признак успешной верификации</returns>
    [HttpPost("confirm-otp")]
    [SwaggerOperation(
        Summary = "Подтвердить OTP-код",
        Description = "Проверяет введённый OTP-код. При успехе помечает телефон пользователя как верифицированный.")]
    [ProducesResponseType(typeof(ConfirmOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmOtp([FromBody] ConfirmOtpRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var command = new ConfirmPhoneOtpCommand(request.VerifyId, request.Token, userId.Value);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new ConfirmOtpResponse(result.Value));
    }
}
