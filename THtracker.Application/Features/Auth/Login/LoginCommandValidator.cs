using FluentValidation;

namespace THtracker.Application.Features.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty().WithMessage("La información del dispositivo es obligatoria.");
    }
}
