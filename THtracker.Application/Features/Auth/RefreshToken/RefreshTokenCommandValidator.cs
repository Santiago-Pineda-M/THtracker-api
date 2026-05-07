using FluentValidation;

namespace THtracker.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio.");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty().WithMessage("La información del dispositivo es obligatoria.");
    }
}
