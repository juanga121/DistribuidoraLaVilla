using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class MovimientoMateriaPrimaDTOValidator : AbstractValidator<MovimientoMateriaPrimaDTO>
    {
        public MovimientoMateriaPrimaDTOValidator()
        {
            // Validación: Cantidad > 0
            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0");

            // Validación: IdLoteMateria válido
            RuleFor(x => x.IdLoteMateria)
                .GreaterThan(0)
                .WithMessage("Debe especificar un lote de materia prima válido");

            // Validación: IdTipoMovimiento válido (1-5)
            RuleFor(x => x.IdTipoMovimiento)
                .InclusiveBetween(1, 5)
                .WithMessage("El tipo de movimiento debe estar entre 1 (Entrada) y 5 (Vencimiento)");

            // Validación: IdUnidadMedida válido
            RuleFor(x => x.IdUnidadMedida)
                .GreaterThan(0)
                .WithMessage("Debe especificar una unidad de medida válida");

            // Validación: IdUsuario no vacío
            RuleFor(x => x.IdUsuario)
                .NotEmpty()
                .WithMessage("Debe especificar un usuario válido");

            // Validación: Observación opcional pero con longitud máxima
            RuleFor(x => x.Observacion)
                .MaximumLength(500)
                .WithMessage("La observación no puede exceder 500 caracteres");
        }
    }
}
