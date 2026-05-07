using FluentValidation;

namespace THtracker.Application.Features.Tasks.Commands.ToggleTaskCompletion;

public sealed class ToggleTaskCompletionCommandValidator : AbstractValidator<ToggleTaskCompletionCommand>
{
    public ToggleTaskCompletionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
