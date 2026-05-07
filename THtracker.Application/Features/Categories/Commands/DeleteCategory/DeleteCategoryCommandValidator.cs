using FluentValidation;

namespace THtracker.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
