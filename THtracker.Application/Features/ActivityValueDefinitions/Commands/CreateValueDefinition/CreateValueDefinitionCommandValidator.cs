using FluentValidation;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.CreateValueDefinition;

public sealed class CreateValueDefinitionCommandValidator : AbstractValidator<CreateValueDefinitionCommand>
{
    public CreateValueDefinitionCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty().WithMessage("La actividad es obligatoria.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.ValueType)
            .NotEmpty().WithMessage("El tipo de valor es obligatorio.")
            .MaximumLength(50);
    }
}
