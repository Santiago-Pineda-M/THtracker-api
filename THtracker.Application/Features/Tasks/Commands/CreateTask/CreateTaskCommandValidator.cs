using FluentValidation;

namespace THtracker.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty().WithMessage("La lista de tareas es obligatoria.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es obligatorio.")
            .MaximumLength(2000);
    }
}
