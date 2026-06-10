using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecibosCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recibos_caja",
                columns: table => new
                {
                    id_recibo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_pago = table.Column<int>(type: "int", nullable: false),
                    numero_recibo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cliente_nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    numero_factura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    monto_pagado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    metodo_pago = table.Column<int>(type: "int", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recibos_caja", x => x.id_recibo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recibos_caja");
        }
    }
}
