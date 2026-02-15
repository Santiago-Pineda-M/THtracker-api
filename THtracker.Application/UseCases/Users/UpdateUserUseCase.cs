using FluentValidation;
using THtracker.Application.DTOs.Users;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class UpdateUserUseCase
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateUserRequest> _validator;

    public UpdateUserUseCase(
        IUserRepository repository, 
        IUnitOfWork unitOfWork,
        IValidator<UpdateUserRequest> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public virtual async Task<Result<UserDto>> ExecuteAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<UserDto>(new Error("Validation", errors));
        }

        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Failure<UserDto>(new Error("NotFound", "User not found."));

        if (user.Email != request.Email && await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<UserDto>(new Error("Conflict", "User with this email already exists."));
        }

        user.Update(request.Name, request.Email);
        
        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Name, user.Email);
    }
}
