using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Application.Interfaces;

namespace THtracker.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<UserResponse>(new Error("Conflict", "El usuario ya existe con este email."));
        }

        var user = new User(request.Name, request.Email);
        string passwordHash = _passwordHasher.Hash(request.Password);
        user.SetPassword(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
