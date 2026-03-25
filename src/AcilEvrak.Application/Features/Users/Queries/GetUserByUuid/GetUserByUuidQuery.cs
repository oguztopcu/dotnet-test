using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Queries.GetUserByUuid;

public sealed record GetUserByUuidQuery(Guid Uuid) : IRequest<Result<UserResponse>>;
