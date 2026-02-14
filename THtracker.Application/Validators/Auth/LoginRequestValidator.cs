using FluentValidation;
using THtracker.Application.DTOs.Auth;

namespace THtracker.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es obligatorio.")
            .EmailAddress()
            .WithMessage("Formato de email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty()
            .WithMessage("La información del dispositivo es obligatoria.");
    }
}
