using FluentValidation;

namespace THtracker.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
