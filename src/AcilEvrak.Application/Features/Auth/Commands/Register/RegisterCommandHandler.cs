using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using AcilEvrak.Domain.ValueObjects;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("VALIDATION_FAILED", "Password must be at least 8 characters.");

        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (existingUser is not null)
            throw new ConflictException("USER_ALREADY_EXISTS", $"A user with email '{email}' already exists.");

        var passwordHash = PasswordHash.FromHash(_passwordHasher.Hash(request.Password));
        var user = User.Create(email, passwordHash, request.FirstName, request.LastName, "User", 0);

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var (_, uuid) = await _userRepository.CreateAsync(user, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<RegisterResponse>.Success(new RegisterResponse(uuid, email, request.FirstName, request.LastName));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
