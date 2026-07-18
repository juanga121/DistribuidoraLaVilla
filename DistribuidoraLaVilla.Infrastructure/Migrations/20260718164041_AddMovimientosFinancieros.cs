using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimientosFinancieros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movimientos_financieros",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_movimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    sub_tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    origen_modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    referencia_id = table.Column<int>(type: "int", nullable: true),
                    fecha_movimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_financieros", x => x.id_movimiento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimientos_financieros");
        }
    }
}
