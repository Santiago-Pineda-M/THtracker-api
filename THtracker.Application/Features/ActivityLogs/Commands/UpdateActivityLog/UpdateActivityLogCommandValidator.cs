using FluentValidation;

namespace THtracker.Application.Features.ActivityLogs.Commands.UpdateActivityLog;

public sealed class UpdateActivityLogCommandValidator : AbstractValidator<UpdateActivityLogCommand>
{
    public UpdateActivityLogCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.StartedAt)
            .Must(d => d != default)
            .WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x)
            .Must(x => !x.EndedAt.HasValue || x.EndedAt >= x.StartedAt)
            .WithMessage("La fecha de fin no puede ser anterior al inicio.");
    }
}
