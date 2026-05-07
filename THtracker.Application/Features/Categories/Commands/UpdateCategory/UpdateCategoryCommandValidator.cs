using FluentValidation;

namespace THtracker.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
