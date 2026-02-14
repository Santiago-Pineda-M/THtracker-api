using FluentValidation;

namespace THtracker.Application.Validators.Roles;

public class RoleNameValidator : AbstractValidator<string>
{
    public RoleNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El nombre del rol no debe exceder 50 caracteres.");
    }
}
