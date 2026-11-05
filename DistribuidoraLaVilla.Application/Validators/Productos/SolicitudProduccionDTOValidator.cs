using DistribuidoraLaVilla.Domain.DTOS.Productos;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators.Productos
{
    public class SolicitudProduccionDTOValidator : AbstractValidator<SolicitudProduccionDTO>
    {
        public SolicitudProduccionDTOValidator()
        {
            RuleFor(x => x.IdProducto)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");

            RuleFor(x => x.CantidadProducir)
                .GreaterThan(0)
                .WithMessage("La cantidad a producir debe ser mayor a 0");

            // La unidad de medida ya no la elige el cliente: se deriva del modo de inventario del
            // producto. Se acepta el valor pero no se exige, para no romper clientes anteriores.
            RuleFor(x => x.IdUnidadMedida)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El ID de la unidad de medida no puede ser negativo");

            RuleFor(x => x.IdUsuario)
                .NotEmpty()
                .WithMessage("El ID del usuario es obligatorio");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));
        }
    }
}
