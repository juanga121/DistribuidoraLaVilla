using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPesoPorUnidadAndPesoDisponible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "peso_por_unidad",
                table: "productos",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_disponible",
                table: "lotes_productos",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            // Seed PesoDisponible for existing lots.
            // For products WITH peso_por_unidad: existing cantidad_disponible is in kg,
            // so peso_disponible = old cantidad_disponible, and cantidad_disponible becomes units.
            migrationBuilder.Sql(@"
                UPDATE lp
                SET 
                    peso_disponible = lp.cantidad_disponible,
                    cantidad_disponible = lp.cantidad_disponible / NULLIF(p.peso_por_unidad, 0)
                FROM lotes_productos lp
                INNER JOIN productos p ON p.id_producto = lp.id_producto
                WHERE p.peso_por_unidad IS NOT NULL AND p.peso_por_unidad > 0;
            ");

            // For products WITHOUT peso_por_unidad: keep backward compatibility
            // (cantidad_disponible stays in kg, peso_disponible = cantidad_disponible)
            migrationBuilder.Sql(@"
                UPDATE lp
                SET peso_disponible = lp.cantidad_disponible
                FROM lotes_productos lp
                INNER JOIN productos p ON p.id_producto = lp.id_producto
                WHERE p.peso_por_unidad IS NULL OR p.peso_por_unidad <= 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "peso_por_unidad",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "peso_disponible",
                table: "lotes_productos");
        }
    }
}
