using FluentValidation;
using THtracker.Application.DTOs.Auth;

namespace THtracker.Application.Validators.Auth;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequest>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Provider).NotEmpty().WithMessage("El proveedor es obligatorio.");

        RuleFor(x => x.Token).NotEmpty().WithMessage("El token es obligatorio.");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty()
            .WithMessage("La información del dispositivo es obligatoria.");
    }
}
