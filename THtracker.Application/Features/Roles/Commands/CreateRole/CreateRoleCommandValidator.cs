using FluentValidation;

namespace THtracker.Application.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(100);
    }
}
