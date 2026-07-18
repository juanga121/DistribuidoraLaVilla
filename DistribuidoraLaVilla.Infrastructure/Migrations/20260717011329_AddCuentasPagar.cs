using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCuentasPagar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cuentas_pagar",
                columns: table => new
                {
                    id_cuenta_pagar = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_orden_compra = table.Column<int>(type: "int", nullable: true),
                    monto_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_vencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_pagar", x => x.id_cuenta_pagar);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_pagar_estado_saldo",
                table: "cuentas_pagar",
                columns: new[] { "estado", "saldo_pendiente" });

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_pagar_proveedor",
                table: "cuentas_pagar",
                column: "id_proveedor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cuentas_pagar");
        }
    }
}
