using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResponse<UserListItem>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResponse<UserListItem>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _userRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(u => new UserListItem(u.Uuid, u.Email, u.FirstName, u.LastName, u.Role, u.IsActive, u.CreatedAt)).ToList().AsReadOnly();
        return Result<PagedResponse<UserListItem>>.Success(PagedResponse<UserListItem>.Create(dtos, request.Page, request.PageSize, totalCount));
    }
}
