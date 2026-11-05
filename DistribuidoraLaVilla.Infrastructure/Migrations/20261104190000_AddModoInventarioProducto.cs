using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModoInventarioProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "modo_inventario",
                table: "productos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Los productos ya marcados para venta por peso pasan a operar en modo Peso.
            migrationBuilder.Sql("UPDATE productos SET modo_inventario = 2 WHERE venta_por_peso = 1;");

            // Normaliza los lotes existentes al nuevo invariante: en los modos por peso el contador
            // autoritativo (cantidad_disponible y cantidad_inicial) se expresa en kilogramos.
            // La migración 20260711180955 había convertido cantidad_disponible de kg a unidades y
            // dejado los kg en peso_disponible; acá se invierte esa conversión para que exista una
            // sola convención en la columna. cantidad_inicial toma el peso total recibido.
            migrationBuilder.Sql(@"
                UPDATE lp
                SET cantidad_disponible = CASE WHEN lp.peso_disponible > 0 THEN lp.peso_disponible ELSE lp.cantidad_disponible END,
                    cantidad_inicial    = CASE WHEN lp.peso_total > 0 THEN lp.peso_total ELSE lp.cantidad_inicial END
                FROM lotes_productos lp
                INNER JOIN productos p ON p.id_producto = lp.id_producto
                WHERE p.modo_inventario IN (2, 3);
            ");

            // En el modo por unidad el lote no administra kilogramos: peso_disponible queda en 0.
            migrationBuilder.Sql(@"
                UPDATE lotes_productos
                SET peso_disponible = 0
                WHERE id_producto IN (SELECT id_producto FROM productos WHERE modo_inventario NOT IN (2, 3));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La normalización de lotes no se revierte: la convención anterior no puede reconstruirse
            // sin ambigüedad. Para volver atrás hay que restaurar un backup previo a esta migración.
            migrationBuilder.DropColumn(
                name: "modo_inventario",
                table: "productos");
        }
    }
}
