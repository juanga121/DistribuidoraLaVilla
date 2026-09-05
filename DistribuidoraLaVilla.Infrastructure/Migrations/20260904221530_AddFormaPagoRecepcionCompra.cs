using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormaPagoRecepcionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "forma_pago",
                table: "recepciones_compra",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "id_recepcion_compra",
                table: "lotes_productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lotes_productos_id_recepcion_compra",
                table: "lotes_productos",
                column: "id_recepcion_compra");

            migrationBuilder.AddForeignKey(
                name: "FK_lotes_productos_recepciones_compra",
                table: "lotes_productos",
                column: "id_recepcion_compra",
                principalTable: "recepciones_compra",
                principalColumn: "id_recepcion_compra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lotes_productos_recepciones_compra",
                table: "lotes_productos");

            migrationBuilder.DropIndex(
                name: "IX_lotes_productos_id_recepcion_compra",
                table: "lotes_productos");

            migrationBuilder.DropColumn(
                name: "forma_pago",
                table: "recepciones_compra");

            migrationBuilder.DropColumn(
                name: "id_recepcion_compra",
                table: "lotes_productos");
        }
    }
}
