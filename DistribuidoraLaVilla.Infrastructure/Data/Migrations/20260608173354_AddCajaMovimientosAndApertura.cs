using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaMovimientosAndApertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caja_apertura",
                columns: table => new
                {
                    id_apertura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha_apertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    monto_final = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_ingresos = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_egresos = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_apertura", x => x.id_apertura);
                });

            migrationBuilder.CreateTable(
                name: "caja_movimientos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_apertura = table.Column<int>(type: "int", nullable: false),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    id_factura = table.Column<int>(type: "int", nullable: true),
                    id_pago = table.Column<int>(type: "int", nullable: true),
                    id_recibo = table.Column<int>(type: "int", nullable: true),
                    concepto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_movimientos", x => x.id_movimiento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caja_apertura");

            migrationBuilder.DropTable(
                name: "caja_movimientos");
        }
    }
}
