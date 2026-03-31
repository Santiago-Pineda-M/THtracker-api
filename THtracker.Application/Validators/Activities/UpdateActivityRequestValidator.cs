using FluentValidation;
using THtracker.Application.DTOs.Activities;

namespace THtracker.Application.Validators.Activities;

public class UpdateActivityRequestValidator : AbstractValidator<UpdateActivityRequest>
{
    public UpdateActivityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la actividad es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("El color debe ser un código hexadecimal válido (ej: #FF0000).");
    }
}
