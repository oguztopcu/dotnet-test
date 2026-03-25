using AcilEvrak.Application.Features.Users.Commands.CreateUser;
using AcilEvrak.Application.Features.Users.Commands.DeleteUser;
using AcilEvrak.Application.Features.Users.Commands.UpdateUser;
using AcilEvrak.Application.Features.Users.Queries.GetUserByUuid;
using AcilEvrak.Application.Features.Users.Queries.GetUsers;
using AcilEvrak.WebAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcilEvrak.WebAPI.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Role);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return StatusCode(201, ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [HttpGet("{uuid:guid}")]
    public async Task<IActionResult> GetByUuid(Guid uuid, CancellationToken cancellationToken)
    {
        var query = new GetUserByUuidQuery(uuid);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(404, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(result.Data, GetCorrelationId()));
    }

    [HttpPut("{uuid:guid}")]
    public async Task<IActionResult> Update(Guid uuid, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(uuid, request.FirstName, request.LastName, request.Role, request.IsActive);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(null, GetCorrelationId()));
    }

    [HttpDelete("{uuid:guid}")]
    public async Task<IActionResult> Delete(Guid uuid, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(uuid);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(400, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!, result.ErrorType!, GetCorrelationId(), result.ErrorDetails));

        return Ok(ApiResponse.Ok(null, GetCorrelationId()));
    }

    private string GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.CreateVersion7().ToString();
}
