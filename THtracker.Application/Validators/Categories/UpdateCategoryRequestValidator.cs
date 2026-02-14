using FluentValidation;
using THtracker.Application.DTOs.Categories;

namespace THtracker.Application.Validators.Categories;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");
    }
}
