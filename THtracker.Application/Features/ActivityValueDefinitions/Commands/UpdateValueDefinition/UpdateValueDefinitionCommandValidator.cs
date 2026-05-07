using FluentValidation;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.UpdateValueDefinition;

public sealed class UpdateValueDefinitionCommandValidator : AbstractValidator<UpdateValueDefinitionCommand>
{
    public UpdateValueDefinitionCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty().WithMessage("La actividad es obligatoria.");
        RuleFor(x => x.DefinitionId).NotEmpty().WithMessage("La definición es obligatoria.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.ValueType)
            .NotEmpty().WithMessage("El tipo de valor es obligatorio.")
            .MaximumLength(50);
    }
}
