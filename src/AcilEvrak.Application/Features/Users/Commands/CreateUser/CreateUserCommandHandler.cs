using System.Text.Json;
using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using AcilEvrak.Domain.ValueObjects;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("VALIDATION_FAILED", "Password must be at least 8 characters.", new { password = "Password must be at least 8 characters." });

        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (existingUser is not null)
            throw new ConflictException("USER_ALREADY_EXISTS", $"A user with email '{email}' already exists.");

        var passwordHash = PasswordHash.FromHash(_passwordHasher.Hash(request.Password));
        var user = User.Create(email, passwordHash, request.FirstName, request.LastName, request.Role, 0);

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var (id, uuid) = await _userRepository.CreateAsync(user, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);

            foreach (var domainEvent in user.DomainEvents)
            {
                await _outboxRepository.AddAsync(
                    domainEvent.EventType,
                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    0,
                    Guid.CreateVersion7().ToString(),
                    domainEvent.OccurredAt,
                    _unitOfWork.Connection,
                    _unitOfWork.Transaction,
                    cancellationToken);
            }
            user.ClearDomainEvents();

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<CreateUserResponse>.Success(new CreateUserResponse(uuid, email, request.FirstName, request.LastName, request.Role));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
