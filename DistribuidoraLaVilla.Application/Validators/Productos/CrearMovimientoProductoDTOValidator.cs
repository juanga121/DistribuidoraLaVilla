using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using FluentValidation;
using System;

namespace DistribuidoraLaVilla.Application.Validators.Productos
{
    public class CrearMovimientoProductoDTOValidator : AbstractValidator<CrearMovimientoProductoDTO>
    {
        public CrearMovimientoProductoDTOValidator()
        {
            RuleFor(x => x.IdLoteProducto)
                .GreaterThan(0)
                .WithMessage("El ID del lote de producto debe ser mayor a 0");

            RuleFor(x => x.TipoMovimiento)
                .InclusiveBetween(1, 5)
                .WithMessage("El tipo de movimiento debe estar entre 1 (Entrada) y 5 (Vencimiento)");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0");

            RuleFor(x => x.IdUnidadMedida)
                .GreaterThan(0)
                .WithMessage("El ID de la unidad de medida debe ser mayor a 0");

            RuleFor(x => x.IdUsuario)
                .NotEmpty()
                .WithMessage("El ID del usuario es obligatorio");

            // Observación obligatoria para Ajuste (3) y Vencimiento (5)
            RuleFor(x => x.Observacion)
                .NotEmpty()
                .WithMessage("La observación es obligatoria para movimientos de tipo Ajuste o Vencimiento")
                .When(x => x.TipoMovimiento == (int)TipoMovimientoProducto.Ajuste || 
                          x.TipoMovimiento == (int)TipoMovimientoProducto.Vencimiento);

            RuleFor(x => x.Observacion)
                .MaximumLength(500)
                .WithMessage("La observación no puede exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Observacion));

            // IdCliente obligatorio para Venta
            RuleFor(x => x.IdCliente)
                .NotEmpty()
                .WithMessage("El ID del cliente es obligatorio para movimientos de tipo Venta")
                .When(x => x.TipoMovimiento == (int)TipoMovimientoProducto.Venta);

            // IdProveedor obligatorio para Entrada
            RuleFor(x => x.IdProveedor)
                .NotEmpty()
                .WithMessage("El ID del proveedor es obligatorio para movimientos de tipo Entrada")
                .When(x => x.TipoMovimiento == (int)TipoMovimientoProducto.Entrada);
        }
    }
}
