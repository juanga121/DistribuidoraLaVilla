using DistribuidoraLaVilla.Domain.DTOS.Productos;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators.Productos
{
    public class RecetaProductoDTOValidator : AbstractValidator<RecetaProductoDTO>
    {
        public RecetaProductoDTOValidator()
        {
            RuleFor(x => x.IdProducto)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");

            RuleFor(x => x.IdMateriaPrima)
                .GreaterThan(0)
                .WithMessage("El ID de la materia prima debe ser mayor a 0");

            RuleFor(x => x.CantidadRequerida)
                .GreaterThan(0)
                .WithMessage("La cantidad requerida debe ser mayor a 0");

            RuleFor(x => x.IdUnidadMedida)
                .GreaterThan(0)
                .WithMessage("El ID de la unidad de medida debe ser mayor a 0");
        }
    }
}
