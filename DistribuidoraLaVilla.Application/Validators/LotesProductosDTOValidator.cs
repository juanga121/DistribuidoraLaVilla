using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class LotesProductosDTOValidator : AbstractValidator<LotesProductosDTO>
    {
        public LotesProductosDTOValidator(bool validarFechaVencimientoFutura = true)
        {
            // The four quantity/price fields are validated per product inventory mode in
            // LotesProductosService, because the validator has no access to the product. Here we only
            // reject negative values.
            RuleFor(x => x.CantidadUnidades)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La cantidad de unidades no puede ser negativa");

            RuleFor(x => x.PesoTotal)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El peso total no puede ser negativo");

            RuleFor(x => x.PrecioUnitario)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El costo unitario no puede ser negativo");

            RuleFor(x => x.PrecioKilo)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El costo por kilo no puede ser negativo");

            RuleFor(x => x.IdProducto)
                .GreaterThan(0)
                .WithMessage("Debe especificar un producto válido");

            RuleFor(x => x.IdMarca)
                .GreaterThan(0)
                .WithMessage("Debe especificar una marca válida");

            RuleFor(x => x.IdProveedor)
                .NotEmpty()
                .WithMessage("Debe especificar un proveedor válido");

            if (validarFechaVencimientoFutura)
            {
                RuleFor(x => x.FechaVencimiento)
                    .GreaterThan(DateTime.Today)
                    .WithMessage("La fecha de vencimiento debe ser posterior a la fecha actual");
            }

            RuleFor(x => x.IdUnidadMedida)
                .GreaterThan(0)
                .WithMessage("Debe especificar una unidad de medida válida");
        }
    }
}
