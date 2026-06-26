using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimientosProductosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movimientos_productos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_lote_producto = table.Column<int>(type: "int", nullable: false),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    fecha_movimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_movimiento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_productos", x => x.id_movimiento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "movimientos_productos");
        }
    }
}
