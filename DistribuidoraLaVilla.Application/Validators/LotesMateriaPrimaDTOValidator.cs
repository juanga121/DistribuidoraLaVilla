using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Validators
{
    public class LotesMateriaPrimaDTOValidator : AbstractValidator<LotesMateriaPrimaDTO>
    {
        public LotesMateriaPrimaDTOValidator()
        {
            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0");

            RuleFor(x => x.CostoUnitario)
                .GreaterThan(0)
                .WithMessage("El costo unitario debe ser mayor a 0");

            RuleFor(x => x.IdMarca)
                .GreaterThan(0)
                .WithMessage("Debe especificar una marca válida");

            RuleFor(x => x.IdMateria)
                .GreaterThan(0)
                .WithMessage("Debe especificar una materia prima válida");

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
