using FluentValidation;

namespace THtracker.Application.Features.TaskLists.Commands.CreateTaskList;

public sealed class CreateTaskListCommandValidator : AbstractValidator<CreateTaskListCommand>
{
    public CreateTaskListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .MaximumLength(50);
    }
}
