using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuranLogix.Application.DTOs.Auth;
using TuranLogix.Application.Features.Auth.Commands;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return Unauthorized(new { error = result.Error.Message });

        return Ok(new { token = result.Value.Token });
    }
}
