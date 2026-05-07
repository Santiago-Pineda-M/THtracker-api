using FluentValidation;

namespace THtracker.Application.Features.ActivityLogs.Commands.StartActivity;

public sealed class StartActivityCommandValidator : AbstractValidator<StartActivityCommand>
{
    public StartActivityCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty().WithMessage("La actividad es obligatoria.");
    }
}
