using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Exceptions;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Queries.GetUserByUuid;

public sealed class GetUserByUuidQueryHandler : IRequestHandler<GetUserByUuidQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUuidQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByUuidQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUuidAsync(request.Uuid, cancellationToken)
            ?? throw new NotFoundException("USER_NOT_FOUND", $"User with uuid '{request.Uuid}' not found.");

        return Result<UserResponse>.Success(new UserResponse(user.Uuid, user.Email, user.FirstName, user.LastName, user.Role, user.IsActive, user.CreatedAt));
    }
}
