using FluentValidation;

namespace THtracker.Application.Features.UserRoles.Commands.AddRoleToUser;

public sealed class AddRoleToUserCommandValidator : AbstractValidator<AddRoleToUserCommand>
{
    public AddRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("El rol es obligatorio.");
    }
}
