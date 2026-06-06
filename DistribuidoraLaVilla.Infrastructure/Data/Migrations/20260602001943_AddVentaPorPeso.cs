using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVentaPorPeso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tipo",
                table: "unidad_medida",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_por_kilo",
                table: "productos",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "venta_por_peso",
                table: "productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "es_venta_por_peso",
                table: "detalle_factura",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo",
                table: "unidad_medida");

            migrationBuilder.DropColumn(
                name: "precio_por_kilo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "venta_por_peso",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "es_venta_por_peso",
                table: "detalle_factura");
        }
    }
}
