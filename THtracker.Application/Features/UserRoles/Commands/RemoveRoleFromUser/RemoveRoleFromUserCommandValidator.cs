using FluentValidation;

namespace THtracker.Application.Features.UserRoles.Commands.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("El rol es obligatorio.");
    }
}
