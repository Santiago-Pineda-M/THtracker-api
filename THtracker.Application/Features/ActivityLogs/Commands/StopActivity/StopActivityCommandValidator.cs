using FluentValidation;

namespace THtracker.Application.Features.ActivityLogs.Commands.StopActivity;

public sealed class StopActivityCommandValidator : AbstractValidator<StopActivityCommand>
{
    public StopActivityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
