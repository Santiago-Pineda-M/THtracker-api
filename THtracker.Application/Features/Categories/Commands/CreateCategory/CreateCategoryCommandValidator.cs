using FluentValidation;

namespace THtracker.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
