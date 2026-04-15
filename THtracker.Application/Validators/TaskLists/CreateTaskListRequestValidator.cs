namespace THtracker.Application.Validators.TaskLists;

using FluentValidation;
using THtracker.Application.DTOs.TaskLists;

public class CreateTaskListRequestValidator : AbstractValidator<CreateTaskListRequest>
{
    public CreateTaskListRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la lista es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("El color debe ser un código hexadecimal válido (ej: #FF0000).")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}
