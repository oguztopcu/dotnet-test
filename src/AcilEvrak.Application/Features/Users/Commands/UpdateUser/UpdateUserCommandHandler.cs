using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUuidAsync(request.Uuid, cancellationToken)
            ?? throw new NotFoundException("USER_NOT_FOUND", $"User with uuid '{request.Uuid}' not found.");

        user.Update(request.FirstName, request.LastName, request.Role, request.IsActive, 0);

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            await _userRepository.UpdateAsync(user, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
