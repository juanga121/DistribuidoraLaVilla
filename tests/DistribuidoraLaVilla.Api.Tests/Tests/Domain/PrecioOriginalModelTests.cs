using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Api.Tests.Tests.Domain
{
    public class PrecioOriginalModelTests
    {
        // NOTE: relational data annotations ([Column]/[Table]) are verified by
        // reflection because the EF InMemory provider ignores relational
        // conventions (GetColumnName/GetColumnType do not reflect them and
        // GetColumnType throws on InMemoryTypeMapping). The SQL Server mapping
        // itself is proven by the generated migration
        // 20260807020601_AddPrecioOriginalDetalleFactura.

        private static System.Reflection.PropertyInfo GetPrecioOriginalProperty()
        {
            var prop = typeof(DetalleFacturaEntity).GetProperty(nameof(DetalleFacturaEntity.PrecioOriginal));
            Assert.NotNull(prop);
            return prop!;
        }

        private static ColumnAttribute GetPrecioOriginalColumnAttribute()
        {
            var prop = GetPrecioOriginalProperty();
            var colAttr = (ColumnAttribute?)prop.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            return colAttr!;
        }

        [Fact]
        public void PrecioOriginal_ShouldMapToColumn_precio_original()
        {
            Assert.Equal("precio_original", GetPrecioOriginalColumnAttribute().Name);
        }

        [Fact]
        public void PrecioOriginal_ShouldBeNullable()
        {
            var prop = GetPrecioOriginalProperty();
            Assert.True(Nullable.GetUnderlyingType(prop.PropertyType) != null ||
                        !prop.PropertyType.IsValueType);
        }

        [Fact]
        public void PrecioOriginal_ShouldBeNullableDecimal()
        {
            Assert.Equal(typeof(decimal?), GetPrecioOriginalProperty().PropertyType);
        }

        [Fact]
        public void PrecioOriginal_ShouldHaveDecimalTypeName_18_2()
        {
            Assert.Equal("decimal(18,2)", GetPrecioOriginalColumnAttribute().TypeName);
        }
    }
}
