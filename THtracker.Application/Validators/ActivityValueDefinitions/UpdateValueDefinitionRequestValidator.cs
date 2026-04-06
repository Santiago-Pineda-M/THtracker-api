using FluentValidation;
using THtracker.Application.DTOs.ActivityValueDefinitions;

namespace THtracker.Application.Validators.ActivityValueDefinitions;

public class UpdateValueDefinitionRequestValidator : AbstractValidator<UpdateValueDefinitionRequest>
{
    private readonly string[] _validTypes = { "Number", "Text", "Boolean", "Time" };

    public UpdateValueDefinitionRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
            .When(x => x.Name != null);

        RuleFor(x => x.ValueType)
            .Must(x => x == null || _validTypes.Contains(x)).WithMessage($"El tipo debe ser uno de: {string.Join(", ", _validTypes)}")
            .When(x => x.ValueType != null);
    }
}