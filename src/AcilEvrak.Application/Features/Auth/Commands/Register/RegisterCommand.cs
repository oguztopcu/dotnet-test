using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Result<RegisterResponse>>;
