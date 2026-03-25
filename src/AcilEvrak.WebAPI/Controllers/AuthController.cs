using AcilEvrak.Application.Features.Auth.Commands.Login;
using AcilEvrak.Application.Features.Auth.Commands.Logout;
using AcilEvrak.Application.Features.Auth.Commands.RefreshToken;
using AcilEvrak.Application.Features.Auth.Commands.Register;
using AcilEvrak.Application.Features.Auth.Commands.RevokeSession;
using AcilEvrak.Application.Features.Auth.Queries.GetSessions;
using AcilEvrak.WebAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcilEvrak.WebAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return StatusCode(201, ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceName, GetIpAddress(), GetUserAgent());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(401, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, null, GetIpAddress(), GetUserAgent());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(401, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sessionUuid = GetSessionUuid();
        var command = new LogoutCommand(sessionUuid);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(null, GetCorrelationId()));
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetSessionsQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [Authorize]
    [HttpDelete("sessions/{uuid:guid}")]
    public async Task<IActionResult> RevokeSession(Guid uuid, CancellationToken cancellationToken)
    {
        var command = new RevokeSessionCommand(uuid);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(null, GetCorrelationId()));
    }

    private string GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.CreateVersion7().ToString();
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? GetUserAgent() => Request.Headers.UserAgent.ToString();

    private Guid GetSessionUuid() =>
        Guid.Parse(User.FindFirst("session_uuid")?.Value
            ?? throw new Domain.Exceptions.UnauthorizedException("INVALID_TOKEN", "Session UUID not found in token."));

    private long GetUserId() =>
        long.Parse(User.FindFirst("user_id")?.Value
            ?? throw new Domain.Exceptions.UnauthorizedException("INVALID_TOKEN", "User ID not found in token."));
}
