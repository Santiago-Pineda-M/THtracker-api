using FluentValidation;
using THtracker.Application.DTOs.Roles;

namespace THtracker.Application.Validators.Roles;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del rol no puede exceder los 50 caracteres.");
    }
}
