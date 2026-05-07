using FluentValidation;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.DeleteValueDefinition;

public sealed class DeleteValueDefinitionCommandValidator : AbstractValidator<DeleteValueDefinitionCommand>
{
    public DeleteValueDefinitionCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty().WithMessage("La actividad es obligatoria.");
        RuleFor(x => x.DefinitionId).NotEmpty().WithMessage("La definición es obligatoria.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
