using FluentValidation;
using THtracker.Application.DTOs.Users;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class CreateUserUseCase
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserUseCase(
        IUserRepository repository, 
        IUnitOfWork unitOfWork,
        IValidator<CreateUserRequest> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public virtual async Task<Result<UserDto>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<UserDto>(new Error("Validation", errors));
        }

        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<UserDto>(new Error("Conflict", "User with this email already exists."));
        }

        var user = new User(request.Name, request.Email);

        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Name, user.Email);
    }
}
