using DistribuidoraLaVilla.Domain.DTOS.Compras;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class CrearOrdenCompraDTOValidator : AbstractValidator<CrearOrdenCompraDTO>
    {
        public CrearOrdenCompraDTOValidator()
        {
            RuleFor(x => x.IdProveedor)
                .NotEmpty().WithMessage("El proveedor es requerido");

            RuleFor(x => x.FechaEmision)
                .NotEmpty().WithMessage("La fecha de emisión es requerida");

            RuleFor(x => x.Detalles)
                .NotEmpty().WithMessage("Debe incluir al menos un detalle");

            RuleForEach(x => x.Detalles).ChildRules(det =>
            {
                det.RuleFor(d => d.IdProducto)
                    .GreaterThan(0).WithMessage("El producto es requerido");

                det.RuleFor(d => d.Cantidad)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

                det.RuleFor(d => d.PrecioUnitario)
                    .GreaterThanOrEqualTo(0).WithMessage("El precio unitario debe ser mayor o igual a 0");
            });
        }
    }
}
