using FluentValidation;

namespace THtracker.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es obligatorio.")
            .MaximumLength(2000);
    }
}
