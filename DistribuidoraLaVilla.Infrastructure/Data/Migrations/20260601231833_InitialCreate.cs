using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La base de datos ya existe con todas las tablas.
            // Esta migración establece el snapshot del modelo para que
            // futuras migraciones detecten cambios correctamente.
            //
            // NOTA: 27 propiedades decimal(18,2) sin precisión explícita.
            // Para producción, configurar HasPrecision en OnModelCreating.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categorias_materia_prima");

            migrationBuilder.DropTable(
                name: "categorias_productos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "cuentas_por_cobrar");

            migrationBuilder.DropTable(
                name: "detalle_factura");

            migrationBuilder.DropTable(
                name: "estados");

            migrationBuilder.DropTable(
                name: "estados_factura");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "lotes_materia_prima");

            migrationBuilder.DropTable(
                name: "lotes_productos");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "materia_prima");

            migrationBuilder.DropTable(
                name: "movimientos");

            migrationBuilder.DropTable(
                name: "movimientos_materia_prima");

            migrationBuilder.DropTable(
                name: "movimientos_productos");

            migrationBuilder.DropTable(
                name: "ordenes_produccion");

            migrationBuilder.DropTable(
                name: "pagos_cuentas");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.DropTable(
                name: "recetas_producto");

            migrationBuilder.DropTable(
                name: "tipos_factura");

            migrationBuilder.DropTable(
                name: "TiposMovimientoMateriaPrima");

            migrationBuilder.DropTable(
                name: "unidad_medida");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
