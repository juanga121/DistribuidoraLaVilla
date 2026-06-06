using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class ClientesDTOValidator : AbstractValidator<ClientesDTO>
    {
        public ClientesDTOValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre no puede estar vacio")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Documento)
                .MaximumLength(20)
                .WithMessage("El documento no puede exceder 20 caracteres");

            RuleFor(x => x.Direccion)
                .MaximumLength(255)
                .WithMessage("La direccion no puede exceder 255 caracteres");

            RuleFor(x => x.Telefono)
                .MaximumLength(20)
                .WithMessage("El telefono no puede exceder 20 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El email no puede estar vacio")
                .MaximumLength(100)
                .WithMessage("El email no puede exceder 100 caracteres")
                .EmailAddress()
                .WithMessage("El formato del email no es valido");
        }
    }
}
