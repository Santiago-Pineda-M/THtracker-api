using FluentValidation;

namespace THtracker.Application.Features.UserSessions.Commands.RevokeSession;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("La sesión es obligatoria.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es obligatorio.");
    }
}
