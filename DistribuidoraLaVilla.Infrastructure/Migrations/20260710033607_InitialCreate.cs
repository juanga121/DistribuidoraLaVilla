using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraLaVilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activos",
                columns: table => new
                {
                    id_activo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activos", x => x.id_activo);
                });

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
                    id_pago = table.Column<int>(type: "int", nullable: true),
                    id_recibo = table.Column<int>(type: "int", nullable: true),
                    concepto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    metodo_pago = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_movimientos", x => x.id_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "categorias_materia_prima",
                columns: table => new
                {
                    id_categoria_materia_prima = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_materia_prima", x => x.id_categoria_materia_prima);
                });

            migrationBuilder.CreateTable(
                name: "categorias_productos",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_productos", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    estado = table.Column<int>(type: "int", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    limite_credito = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    dias_credito = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "cuentas_por_cobrar",
                columns: table => new
                {
                    id_cxc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_factura = table.Column<int>(type: "int", nullable: true),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    fecha_emision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fecha_vencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    monto_total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    saldo_pendiente = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_por_cobrar", x => x.id_cxc);
                });

            migrationBuilder.CreateTable(
                name: "detalle_compra",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_orden_compra = table.Column<int>(type: "int", nullable: false),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_compra", x => x.id_detalle);
                });

            migrationBuilder.CreateTable(
                name: "detalle_factura",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_factura = table.Column<int>(type: "int", nullable: true),
                    id_producto = table.Column<int>(type: "int", nullable: true),
                    id_lote = table.Column<int>(type: "int", nullable: true),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: true),
                    precio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    es_venta_por_peso = table.Column<bool>(type: "bit", nullable: true),
                    peso_total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    precio_kilo = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_factura", x => x.id_detalle);
                });

            migrationBuilder.CreateTable(
                name: "estados",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados", x => x.id_estado);
                });

            migrationBuilder.CreateTable(
                name: "estados_factura",
                columns: table => new
                {
                    id_estado_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_factura", x => x.id_estado_factura);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tipo_factura = table.Column<int>(type: "int", nullable: true),
                    forma_pago = table.Column<int>(type: "int", nullable: true),
                    metodo_pago = table.Column<int>(type: "int", nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id_factura);
                });

            migrationBuilder.CreateTable(
                name: "lotes_materia_prima",
                columns: table => new
                {
                    id_lote_materia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_marca = table.Column<int>(type: "int", nullable: false),
                    id_materia = table.Column<int>(type: "int", nullable: false),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha_entrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    costo_unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    costo_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cantidad_inicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cantidad_disponible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lotes_materia_prima", x => x.id_lote_materia);
                });

            migrationBuilder.CreateTable(
                name: "lotes_productos",
                columns: table => new
                {
                    id_lote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha_entrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad_unidades = table.Column<int>(type: "int", nullable: false),
                    peso_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    precio_kilo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    precio_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_marca = table.Column<int>(type: "int", nullable: false),
                    cantidad_inicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cantidad_disponible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lotes_productos", x => x.id_lote);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    id_marca = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    estado = table.Column<int>(type: "int", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.id_marca);
                });

            migrationBuilder.CreateTable(
                name: "materia_prima",
                columns: table => new
                {
                    id_materia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_categoria_materia_prima = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materia_prima", x => x.id_materia);
                });

            migrationBuilder.CreateTable(
                name: "movimientos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_lote_producto = table.Column<int>(type: "int", nullable: true),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    fecha_movimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_movimiento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    id_entidad = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos", x => x.id_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_materia_prima",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_lote_materia = table.Column<int>(type: "int", nullable: false),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_materia_prima", x => x.id_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_productos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_lote_producto = table.Column<int>(type: "int", nullable: false),
                    tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    fecha_movimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_movimiento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_productos", x => x.id_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "ordenes_compra",
                columns: table => new
                {
                    id_orden_compra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    impuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes_compra", x => x.id_orden_compra);
                });

            migrationBuilder.CreateTable(
                name: "ordenes_produccion",
                columns: table => new
                {
                    id_orden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    cantidad_producir = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    fecha_orden = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_completada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_lote_generado = table.Column<int>(type: "int", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes_produccion", x => x.id_orden);
                });

            migrationBuilder.CreateTable(
                name: "pagos_cuentas",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cxc = table.Column<int>(type: "int", nullable: true),
                    fecha_pago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    monto_pago = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    metodo_pago = table.Column<int>(type: "int", nullable: true),
                    referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    observacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos_cuentas", x => x.id_pago);
                });

            migrationBuilder.CreateTable(
                name: "pasivos",
                columns: table => new
                {
                    id_pasivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pasivos", x => x.id_pasivo);
                });

            migrationBuilder.CreateTable(
                name: "patrimonio",
                columns: table => new
                {
                    id_patrimonio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrimonio", x => x.id_patrimonio);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    id_categoria = table.Column<int>(type: "int", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    venta_por_peso = table.Column<bool>(type: "bit", nullable: false),
                    precio_por_kilo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id_producto);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id_proveedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    tipo_proveedor = table.Column<int>(type: "int", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id_proveedor);
                });

            migrationBuilder.CreateTable(
                name: "recetas_producto",
                columns: table => new
                {
                    id_receta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    id_materia_prima = table.Column<int>(type: "int", nullable: false),
                    cantidad_requerida = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_unidad_medida = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recetas_producto", x => x.id_receta);
                });

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

            migrationBuilder.CreateTable(
                name: "tipos_factura",
                columns: table => new
                {
                    id_tipo_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_factura", x => x.id_tipo_factura);
                });

            migrationBuilder.CreateTable(
                name: "TiposMovimientoMateriaPrima",
                columns: table => new
                {
                    id_tipo_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMovimientoMateriaPrima", x => x.id_tipo_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "unidad_medida",
                columns: table => new
                {
                    id_unidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    simbolo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unidad_medida", x => x.id_unidad);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rol = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activos");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "caja_apertura");

            migrationBuilder.DropTable(
                name: "caja_movimientos");

            migrationBuilder.DropTable(
                name: "categorias_materia_prima");

            migrationBuilder.DropTable(
                name: "categorias_productos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "cuentas_por_cobrar");

            migrationBuilder.DropTable(
                name: "detalle_compra");

            migrationBuilder.DropTable(
                name: "detalle_factura");

            migrationBuilder.DropTable(
                name: "estados");

            migrationBuilder.DropTable(
                name: "estados_factura");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "lotes_materia_prima");

            migrationBuilder.DropTable(
                name: "lotes_productos");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "materia_prima");

            migrationBuilder.DropTable(
                name: "movimientos");

            migrationBuilder.DropTable(
                name: "movimientos_materia_prima");

            migrationBuilder.DropTable(
                name: "movimientos_productos");

            migrationBuilder.DropTable(
                name: "ordenes_compra");

            migrationBuilder.DropTable(
                name: "ordenes_produccion");

            migrationBuilder.DropTable(
                name: "pagos_cuentas");

            migrationBuilder.DropTable(
                name: "pasivos");

            migrationBuilder.DropTable(
                name: "patrimonio");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.DropTable(
                name: "recetas_producto");

            migrationBuilder.DropTable(
                name: "recibos_caja");

            migrationBuilder.DropTable(
                name: "tipos_factura");

            migrationBuilder.DropTable(
                name: "TiposMovimientoMateriaPrima");

            migrationBuilder.DropTable(
                name: "unidad_medida");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
