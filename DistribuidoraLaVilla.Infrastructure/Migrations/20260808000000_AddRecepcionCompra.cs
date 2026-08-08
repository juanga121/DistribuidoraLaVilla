using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    public partial class AddRecepcionCompra : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recepciones_compra",
                columns: table => new
                {
                    id_recepcion_compra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_orden_compra = table.Column<int>(type: "int", nullable: false),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    numero_factura_proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fecha_factura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_vencimiento_lotes = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_marca = table.Column<int>(type: "int", nullable: false),
                    monto_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recepciones_compra", x => x.id_recepcion_compra);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recepciones_compra_orden",
                table: "recepciones_compra",
                column: "id_orden_compra",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recepciones_compra");
        }
    }
}
