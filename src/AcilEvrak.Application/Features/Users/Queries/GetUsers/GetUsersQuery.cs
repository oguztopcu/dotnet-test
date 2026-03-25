using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResponse<UserListItem>>>;
