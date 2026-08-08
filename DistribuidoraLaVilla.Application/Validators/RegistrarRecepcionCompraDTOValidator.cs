using DistribuidoraLaVilla.Domain.DTOS.Compras;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class RegistrarRecepcionCompraDTOValidator : AbstractValidator<RegistrarRecepcionCompraDTO>
    {
        public RegistrarRecepcionCompraDTOValidator()
        {
            RuleFor(x => x.NumeroFacturaProveedor)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Debe especificar el número de factura del proveedor");

            RuleFor(x => x.FechaFactura)
                .NotEmpty()
                .WithMessage("Debe especificar la fecha de la factura");

            RuleFor(x => x.FechaRecepcion)
                .NotEmpty()
                .WithMessage("Debe especificar la fecha de recepción");

            RuleFor(x => x.FechaVencimiento)
                .NotEmpty()
                .GreaterThan(x => x.FechaRecepcion)
                .WithMessage("La fecha de vencimiento debe ser posterior a la fecha de recepción");

            RuleFor(x => x.FechaVencimientoLotes)
                .NotEmpty()
                .GreaterThan(x => x.FechaRecepcion)
                .WithMessage("La fecha de vencimiento de los lotes debe ser posterior a la recepción");

            RuleFor(x => x.IdMarca)
                .GreaterThan(0)
                .WithMessage("Debe especificar una marca válida");

            RuleFor(x => x.Detalles)
                .NotEmpty()
                .WithMessage("Debe incluir al menos un detalle de recepción");

            RuleForEach(x => x.Detalles).Must((parent, detalle) => detalle.FechaVencimientoLote > parent.FechaRecepcion)
                .WithMessage("La fecha de vencimiento del lote debe ser posterior a la recepción");

            RuleForEach(x => x.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(x => x.IdProducto)
                    .GreaterThan(0)
                    .WithMessage("Debe especificar un producto válido");

                detalle.RuleFor(x => x.CantidadUnidades)
                    .GreaterThan(0)
                    .WithMessage("La cantidad de unidades debe ser mayor a 0");

                detalle.RuleFor(x => x.PesoTotal)
                    .GreaterThan(0)
                    .WithMessage("El peso total debe ser mayor a 0");

                detalle.RuleFor(x => x.IdUnidadMedida)
                    .GreaterThan(0)
                    .WithMessage("Debe especificar una unidad de medida válida");

                detalle.RuleFor(x => x.PrecioUnitario)
                    .GreaterThan(0)
                    .WithMessage("El costo unitario debe ser mayor a 0");

                detalle.RuleFor(x => x.PrecioKilo)
                    .GreaterThan(0)
                    .WithMessage("El costo por kilo debe ser mayor a 0");
            });
        }
    }
}
