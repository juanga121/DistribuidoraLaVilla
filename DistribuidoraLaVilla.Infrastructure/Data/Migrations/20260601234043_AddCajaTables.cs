using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaTables : Migration
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
                    table.ForeignKey(
                        name: "FK_caja_apertura_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
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
                    concepto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_movimientos", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_caja_apertura_id_apertura",
                        column: x => x.id_apertura,
                        principalTable: "caja_apertura",
                        principalColumn: "id_apertura",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_facturas_id_factura",
                        column: x => x.id_factura,
                        principalTable: "facturas",
                        principalColumn: "id_factura",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_caja_apertura_id_usuario",
                table: "caja_apertura",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_id_apertura",
                table: "caja_movimientos",
                column: "id_apertura");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_id_factura",
                table: "caja_movimientos",
                column: "id_factura");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_id_usuario",
                table: "caja_movimientos",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // caja_movimientos must be dropped first (FK depends on caja_apertura)
            migrationBuilder.DropTable(
                name: "caja_movimientos");

            migrationBuilder.DropTable(
                name: "caja_apertura");
        }
    }
}
