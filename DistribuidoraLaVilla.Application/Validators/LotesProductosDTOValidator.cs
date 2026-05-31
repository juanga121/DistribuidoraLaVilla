using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class LotesProductosDTOValidator : AbstractValidator<LotesProductosDTO>
    {
        public LotesProductosDTOValidator()
        {
            RuleFor(x => x.CantidadUnidades)
                .GreaterThan(0)
                .WithMessage("La cantidad de unidades debe ser mayor a 0");

            RuleFor(x => x.PesoTotal)
                .GreaterThan(0)
                .WithMessage("El peso total debe ser mayor a 0");

            RuleFor(x => x.PrecioUnitario)
                .GreaterThan(0)
                .WithMessage("El precio unitario debe ser mayor a 0");

            RuleFor(x => x.PrecioKilo)
                .GreaterThan(0)
                .WithMessage("El precio por kilo debe ser mayor a 0");

            RuleFor(x => x.IdProducto)
                .GreaterThan(0)
                .WithMessage("Debe especificar un producto válido");

            RuleFor(x => x.IdMarca)
                .GreaterThan(0)
                .WithMessage("Debe especificar una marca válida");

            RuleFor(x => x.IdProveedor)
                .NotEmpty()
                .WithMessage("Debe especificar un proveedor válido");

            RuleFor(x => x.FechaVencimiento)
                .GreaterThan(DateTime.Now)
                .WithMessage("La fecha de vencimiento debe ser posterior a la fecha actual");

            RuleFor(x => x.IdUnidadMedida)
                .GreaterThan(0)
                .WithMessage("Debe especificar una unidad de medida válida");
        }
    }
}
