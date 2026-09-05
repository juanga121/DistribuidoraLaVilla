using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosCxp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagos_cxp",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta_pagar = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    metodo_pago = table.Column<int>(type: "int", nullable: true),
                    observacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos_cxp", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_pagos_cxp_cuentas_pagar",
                        column: x => x.id_cuenta_pagar,
                        principalTable: "cuentas_pagar",
                        principalColumn: "id_cuenta_pagar",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pagos_cxp_cuenta_pagar",
                table: "pagos_cxp",
                column: "id_cuenta_pagar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagos_cxp");
        }
    }
}
