using FluentValidation;

namespace THtracker.Application.Features.Activities.Commands.CreateActivity;

public sealed class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
