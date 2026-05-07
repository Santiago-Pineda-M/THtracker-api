using FluentValidation;

namespace THtracker.Application.Features.Activities.Commands.UpdateActivity;

public sealed class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
