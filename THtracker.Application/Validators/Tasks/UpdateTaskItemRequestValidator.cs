namespace THtracker.Application.Validators.Tasks;

using FluentValidation;
using THtracker.Application.DTOs.Tasks;

public class UpdateTaskItemRequestValidator : AbstractValidator<UpdateTaskItemRequest>
{
    public UpdateTaskItemRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido de la tarea es obligatorio.")
            .MaximumLength(500).WithMessage("El contenido no puede exceder los 500 caracteres.");

        RuleFor(x => x.DueDate)
            .Must(date => date == null || date > DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("La fecha límite debe ser una fecha futura.")
            .When(x => x.DueDate.HasValue);
    }
}
