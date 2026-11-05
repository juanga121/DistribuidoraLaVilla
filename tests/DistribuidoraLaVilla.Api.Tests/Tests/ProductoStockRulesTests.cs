using DistribuidoraLaVilla.Application.Services.Inventario;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using FluentAssertions;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class ProductoStockRulesTests
    {
        [Theory]
        [InlineData((int)ModoInventarioEnum.Unidad, 10)]
        [InlineData((int)ModoInventarioEnum.Peso, 20)]
        public void ResolverStockMinimo_SinValorPropio_UsaElDefaultDelModo(int modo, decimal esperado)
        {
            var resultado = ProductoStockRules.ResolverStockMinimo(modo, null);

            resultado.Should().Be(esperado);
        }

        [Theory]
        [InlineData((int)ModoInventarioEnum.Unidad)]
        [InlineData((int)ModoInventarioEnum.Peso)]
        public void ResolverStockMinimo_ConValorPropio_LoRespeta(int modo)
        {
            var resultado = ProductoStockRules.ResolverStockMinimo(modo, 3.5m);

            resultado.Should().Be(3.5m);
        }

        [Fact]
        public void ResolverStockMinimo_ConCeroExplicito_NoUsaElDefault()
        {
            var resultado = ProductoStockRules.ResolverStockMinimo((int)ModoInventarioEnum.Peso, 0m);

            resultado.Should().Be(0m);
        }

        [Theory]
        [InlineData((int)ModoInventarioEnum.Unidad, false)]
        [InlineData((int)ModoInventarioEnum.Peso, true)]
        public void OperaPorPeso_DependeDelModo(int modo, bool esperado)
        {
            var producto = new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto",
                ModoInventario = modo,
                VentaPorPeso = esperado
            };

            ProductoStockRules.OperaPorPeso(producto).Should().Be(esperado);
        }

        [Fact]
        public void OperaPorPeso_ModoUnidadConVentaPorPesoLegacy_SigueOperandoPorPeso()
        {
            // Filas viejas: modo 1 pero venta_por_peso todavía en true.
            var producto = new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto legacy",
                ModoInventario = (int)ModoInventarioEnum.Unidad,
                VentaPorPeso = true
            };

            ProductoStockRules.OperaPorPeso(producto).Should().BeTrue();
        }
    }
}
