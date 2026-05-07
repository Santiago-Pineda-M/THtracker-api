using FluentValidation;

namespace THtracker.Application.Features.UserRoles.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.RoleNames)
            .NotNull().WithMessage("La lista de roles es obligatoria.");
    }
}
