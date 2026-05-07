using FluentValidation;

namespace THtracker.Application.Features.TaskLists.Commands.UpdateTaskList;

public sealed class UpdateTaskListCommandValidator : AbstractValidator<UpdateTaskListCommand>
{
    public UpdateTaskListCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
