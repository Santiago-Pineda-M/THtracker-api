using FluentValidation;

namespace THtracker.Application.Features.TaskLists.Commands.DeleteTaskList;

public sealed class DeleteTaskListCommandValidator : AbstractValidator<DeleteTaskListCommand>
{
    public DeleteTaskListCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
