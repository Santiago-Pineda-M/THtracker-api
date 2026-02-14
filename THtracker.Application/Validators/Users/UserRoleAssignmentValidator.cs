using FluentValidation;

namespace THtracker.Application.Validators.Users;

public class UserRoleAssignmentValidator : AbstractValidator<(Guid userId, Guid roleId)>
{
    public UserRoleAssignmentValidator()
    {
        RuleFor(x => x.userId).NotEmpty().WithMessage("El id de usuario es obligatorio.");
        RuleFor(x => x.roleId).NotEmpty().WithMessage("El id de rol es obligatorio.");
    }
}
