using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMovimientosMateriaPrimaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "movimietos_materia_prima",
                newName: "movimientos_materia_prima");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "movimientos_materia_prima",
                newName: "movimietos_materia_prima");
        }
    }
}
