using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixLotesProductosNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix existing NULL values before altering columns
            migrationBuilder.Sql(@"
                UPDATE lotes_productos SET id_marca = 1 WHERE id_marca IS NULL;
                UPDATE lotes_productos SET id_proveedor = 'f2a559d1-d032-475f-a9cb-fa4156c39b67' WHERE id_proveedor IS NULL;
            ");

            // Alter all nullable columns to NOT NULL
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_producto int NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_proveedor uniqueidentifier NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN fecha_entrada datetime NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN fecha_vencimiento datetime NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_unidades int NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN peso_total decimal(18,2) NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_unidad_medida int NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_unitario decimal(18,2) NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_kilo decimal(18,2) NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_total decimal(18,2) NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_marca int NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN estado int NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_inicial decimal(18,2) NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_disponible decimal(18,2) NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert columns back to nullable
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_producto int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_proveedor uniqueidentifier NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN fecha_entrada datetime NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN fecha_vencimiento datetime NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_unidades int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN peso_total decimal(18,2) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_unidad_medida int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_unitario decimal(18,2) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_kilo decimal(18,2) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN precio_total decimal(18,2) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN id_marca int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN estado int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_inicial decimal(18,2) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE lotes_productos ALTER COLUMN cantidad_disponible decimal(18,2) NULL;");
        }
    }
}
