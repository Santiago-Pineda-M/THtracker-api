using FluentValidation;
using THtracker.Application.DTOs.ActivityValueDefinitions;

namespace THtracker.Application.Validators.ActivityValueDefinitions;

public class CreateValueDefinitionRequestValidator : AbstractValidator<CreateValueDefinitionRequest>
{
    private readonly string[] _validTypes = { "Number", "Text", "Boolean", "Time" };

    public CreateValueDefinitionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

        RuleFor(x => x.ValueType)
            .NotEmpty().WithMessage("El tipo de valor es obligatorio")
            .Must(x => _validTypes.Contains(x)).WithMessage($"El tipo debe ser uno de: {string.Join(", ", _validTypes)}");
    }
}
