using FluentValidation;

namespace THtracker.Application.Features.ActivityLogValues.Commands.SaveLogValues;

public sealed class SaveLogValuesCommandValidator : AbstractValidator<SaveLogValuesCommand>
{
    public SaveLogValuesCommandValidator()
    {
        RuleFor(x => x.ActivityLogId).NotEmpty().WithMessage("El registro de actividad es obligatorio.");

        RuleFor(x => x.Values)
            .NotNull().WithMessage("Los valores son obligatorios.");

        RuleForEach(x => x.Values).ChildRules(item =>
        {
            item.RuleFor(i => i.ValueDefinitionId).NotEmpty();
            item.RuleFor(i => i.Value).NotEmpty().MaximumLength(4000);
        });
    }
}
