using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUuidAsync(request.Uuid, cancellationToken)
            ?? throw new NotFoundException("USER_NOT_FOUND", $"User with uuid '{request.Uuid}' not found.");

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            await _userRepository.SoftDeleteAsync(user.Id, 0, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
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
