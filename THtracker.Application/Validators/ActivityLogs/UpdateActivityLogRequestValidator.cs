using FluentValidation;
using THtracker.Application.DTOs.ActivityLogs;

namespace THtracker.Application.Validators.ActivityLogs;

public class UpdateActivityLogRequestValidator : AbstractValidator<UpdateActivityLogRequest>
{
    public UpdateActivityLogRequestValidator()
    {
        RuleFor(x => x.StartedAt)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de inicio no puede ser en el futuro");

        RuleFor(x => x.EndedAt)
            .GreaterThan(x => x.StartedAt)
            .When(x => x.EndedAt.HasValue)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio");
    }
}
