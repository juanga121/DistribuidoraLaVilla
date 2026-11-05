using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModoPesoVariable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El modo 3 ("Peso variable") no se diferenciaba del modo 2 en stock ni en venta.
            // Los productos existentes pasan a modo 2 y el valor 3 queda reservado.
            migrationBuilder.Sql("UPDATE productos SET modo_inventario = 2 WHERE modo_inventario = 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No hay vuelta atrás: el modo 3 se eliminó del modelo y no se puede reconstruir
            // qué productos lo tenían.
        }
    }
}
