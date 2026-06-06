using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaRemoveCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // caja_movimientos must be dropped first (FK depends on caja_apertura)
            migrationBuilder.DropTable(
                name: "caja_movimientos");

            migrationBuilder.DropTable(
                name: "caja_apertura");

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_entidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    detalle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.id_auditoria);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.CreateTable(
                name: "caja_apertura",
                columns: table => new
                {
                    id_apertura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_apertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    monto_final = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    monto_inicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_egresos = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_ingresos = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_caja_apertura_id_usuario",
                table: "caja_apertura",
                column: "id_usuario");

            migrationBuilder.CreateTable(
                name: "caja_movimientos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    concepto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_apertura = table.Column<int>(type: "int", nullable: false),
                    id_factura = table.Column<int>(type: "int", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false)
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
    }
}
