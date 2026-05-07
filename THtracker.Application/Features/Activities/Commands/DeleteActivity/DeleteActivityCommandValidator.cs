using FluentValidation;

namespace THtracker.Application.Features.Activities.Commands.DeleteActivity;

public sealed class DeleteActivityCommandValidator : AbstractValidator<DeleteActivityCommand>
{
    public DeleteActivityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
