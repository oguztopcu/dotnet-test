using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role) : IRequest<Result<CreateUserResponse>>;
