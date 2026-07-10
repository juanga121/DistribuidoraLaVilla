using DistribuidoraLaVilla.Domain.Entities.Compras;
using DistribuidoraLaVilla.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Api.Tests.Tests.Domain
{
    public class ComprasEntityTests
    {
        // ── T-001: EstadoCompraEnum ──

        [Fact]
        public void EstadoCompraEnum_ShouldHaveExactlyFourMembers()
        {
            var values = Enum.GetValues<EstadoCompraEnum>();
            Assert.Equal(4, values.Length);
        }

        [Fact]
        public void EstadoCompraEnum_Pendiente_ShouldHaveValue1()
        {
            Assert.Equal(1, (int)EstadoCompraEnum.Pendiente);
        }

        [Fact]
        public void EstadoCompraEnum_Aprobada_ShouldHaveValue2()
        {
            Assert.Equal(2, (int)EstadoCompraEnum.Aprobada);
        }

        [Fact]
        public void EstadoCompraEnum_Recibida_ShouldHaveValue3()
        {
            Assert.Equal(3, (int)EstadoCompraEnum.Recibida);
        }

        [Fact]
        public void EstadoCompraEnum_Cancelada_ShouldHaveValue4()
        {
            Assert.Equal(4, (int)EstadoCompraEnum.Cancelada);
        }

        // ── T-002: OrdenCompraEntity ──

        [Fact]
        public void OrdenCompraEntity_ShouldHaveTableAttribute_OrdenesCompra()
        {
            var attr = (TableAttribute?)typeof(OrdenCompraEntity)
                .GetCustomAttributes(typeof(TableAttribute), false)
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("ordenes_compra", attr!.Name);
        }

        [Fact]
        public void OrdenCompraEntity_Id_ShouldHaveKeyAndColumnAttributes()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Id));
            Assert.NotNull(prop);

            var keyAttr = prop!.GetCustomAttributes(typeof(KeyAttribute), false);
            Assert.Single(keyAttr);

            var colAttr = (ColumnAttribute?)prop.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("id_orden_compra", colAttr!.Name);
        }

        [Fact]
        public void OrdenCompraEntity_IdProveedor_ShouldBeGuid()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.IdProveedor));
            Assert.NotNull(prop);
            Assert.Equal(typeof(Guid), prop!.PropertyType);
        }

        [Fact]
        public void OrdenCompraEntity_FechaRecepcion_ShouldBeNullable()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.FechaRecepcion));
            Assert.NotNull(prop);
            Assert.True(Nullable.GetUnderlyingType(prop!.PropertyType) != null ||
                        !prop.PropertyType.IsValueType);
        }

        [Fact]
        public void OrdenCompraEntity_Estado_ShouldBeInt()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Estado));
            Assert.NotNull(prop);
            Assert.Equal(typeof(int), prop!.PropertyType);
        }

        [Fact]
        public void OrdenCompraEntity_Subtotal_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Subtotal));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void OrdenCompraEntity_Descuento_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Descuento));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void OrdenCompraEntity_Impuesto_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Impuesto));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void OrdenCompraEntity_Total_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Total));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void OrdenCompraEntity_Observaciones_ShouldHaveStringLength500()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Observaciones));
            Assert.NotNull(prop);

            var attr = (StringLengthAttribute?)prop!.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault();
            Assert.NotNull(attr);
            Assert.Equal(500, attr!.MaximumLength);
        }

        [Fact]
        public void OrdenCompraEntity_Observaciones_ShouldBeNullable()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.Observaciones));
            Assert.NotNull(prop);
            Assert.True(Nullable.GetUnderlyingType(prop!.PropertyType) != null ||
                        !prop.PropertyType.IsValueType);
        }

        [Fact]
        public void OrdenCompraEntity_FechaActualizacion_ShouldBeNullable()
        {
            var prop = typeof(OrdenCompraEntity).GetProperty(nameof(OrdenCompraEntity.FechaActualizacion));
            Assert.NotNull(prop);
            Assert.True(Nullable.GetUnderlyingType(prop!.PropertyType) != null ||
                        !prop.PropertyType.IsValueType);
        }

        // ── T-003: DetalleCompraEntity ──

        [Fact]
        public void DetalleCompraEntity_ShouldHaveTableAttribute()
        {
            var attr = (TableAttribute?)typeof(DetalleCompraEntity)
                .GetCustomAttributes(typeof(TableAttribute), false)
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("detalle_compra", attr!.Name);
        }

        [Fact]
        public void DetalleCompraEntity_Id_ShouldHaveKeyAndColumnAttributes()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.Id));
            Assert.NotNull(prop);

            var keyAttr = prop!.GetCustomAttributes(typeof(KeyAttribute), false);
            Assert.Single(keyAttr);

            var colAttr = (ColumnAttribute?)prop.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("id_detalle", colAttr!.Name);
        }

        [Fact]
        public void DetalleCompraEntity_IdOrdenCompra_ShouldBeInt()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.IdOrdenCompra));
            Assert.NotNull(prop);
            Assert.Equal(typeof(int), prop!.PropertyType);
        }

        [Fact]
        public void DetalleCompraEntity_IdProducto_ShouldBeInt()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.IdProducto));
            Assert.NotNull(prop);
            Assert.Equal(typeof(int), prop!.PropertyType);
        }

        [Fact]
        public void DetalleCompraEntity_Cantidad_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.Cantidad));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void DetalleCompraEntity_PrecioUnitario_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.PrecioUnitario));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        [Fact]
        public void DetalleCompraEntity_Subtotal_ShouldHaveDecimalTypeName()
        {
            var prop = typeof(DetalleCompraEntity).GetProperty(nameof(DetalleCompraEntity.Subtotal));
            Assert.NotNull(prop);

            var colAttr = (ColumnAttribute?)prop!.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(colAttr);
            Assert.Equal("decimal(18,2)", colAttr!.TypeName);
        }

        // ── T-004: DataContext DbSets ──
        // Note: These tests require DataContext from Infrastructure project.
        // They are placed here if referencing infrastructure from test project.
        // The test project already references Infrastructure.
    }
}
