using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientesColumnasCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La entidad ClientesEntity tiene estas columnas pero la tabla física
            // de la BD fue creada sin ellas (database-first snapshot vacío).
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "clientes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime?>(
                name: "fecha_actualizacion",
                table: "clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "limite_credito",
                table: "clientes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dias_credito",
                table: "clientes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dias_credito",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "limite_credito",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "fecha_actualizacion",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "clientes");
        }
    }
}
