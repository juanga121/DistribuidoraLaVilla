-- Borra los datos transaccionales dejando la base lista para probar desde cero.
--
-- CONSERVA:
--   - Lookups: estados, estados_factura, tipos_factura, TiposMovimientoMateriaPrima, unidad_medida
--   - Historial de migraciones: __EFMigrationsHistory
--   - Usuarios: usuarios
--   - Maestros: categorias_productos, categorias_materia_prima, Marcas, proveedores, clientes,
--               productos, materia_prima, recetas_producto
--   - Definiciones financieras: activos, pasivos, patrimonio
--
-- BORRA: todo lo transaccional (compras, recepciones, lotes, movimientos, facturas, caja,
--         cuentas por cobrar/pagar, pagos, produccion, auditoria, recibos).
--
-- Uso:  sqlcmd -S localhost -E -C -d DistribuidoraLaVilla -i scripts\limpiar_datos_transaccionales.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @TablesToClear TABLE (TableName sysname NOT NULL PRIMARY KEY);

    INSERT INTO @TablesToClear (TableName)
    VALUES
        ('detalle_factura'),
        ('detalle_compra'),
        ('movimientos'),
        ('movimientos_productos'),
        ('movimientos_materia_prima'),
        ('movimientos_financieros'),
        ('pagos_cuentas'),
        ('pagos_cxp'),
        ('recibos_caja'),
        ('caja_movimientos'),
        ('caja_apertura'),
        ('recepciones_compra'),
        ('ordenes_compra'),
        ('facturas'),
        ('cuentas_por_cobrar'),
        ('cuentas_pagar'),
        ('ordenes_produccion'),
        ('lotes_productos'),
        ('lotes_materia_prima'),
        ('auditoria');

    DECLARE @SchemaName sysname;
    DECLARE @TableName sysname;
    DECLARE @QualifiedName nvarchar(300);
    DECLARE @Sql nvarchar(max);

    -- Deshabilitar constraints para no depender del orden de borrado.
    DECLARE disable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN @TablesToClear c ON c.TableName = t.name
        WHERE t.is_ms_shipped = 0;

    OPEN disable_cursor;
    FETCH NEXT FROM disable_cursor INTO @SchemaName, @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @QualifiedName = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);
        SET @Sql = N'ALTER TABLE ' + @QualifiedName + N' NOCHECK CONSTRAINT ALL;';
        EXEC sys.sp_executesql @Sql;

        FETCH NEXT FROM disable_cursor INTO @SchemaName, @TableName;
    END

    CLOSE disable_cursor;
    DEALLOCATE disable_cursor;

    -- Borrar y reiniciar identidades.
    DECLARE delete_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN @TablesToClear c ON c.TableName = t.name
        WHERE t.is_ms_shipped = 0;

    OPEN delete_cursor;
    FETCH NEXT FROM delete_cursor INTO @SchemaName, @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @QualifiedName = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);
        SET @Sql = N'DELETE FROM ' + @QualifiedName + N';';
        EXEC sys.sp_executesql @Sql;

        IF EXISTS (
            SELECT 1
            FROM sys.identity_columns ic
            WHERE ic.object_id = OBJECT_ID(@QualifiedName)
        )
        BEGIN
            SET @Sql = N'DBCC CHECKIDENT (' + QUOTENAME(@QualifiedName, '''') + N', RESEED, 0);';
            EXEC sys.sp_executesql @Sql;
        END

        FETCH NEXT FROM delete_cursor INTO @SchemaName, @TableName;
    END

    CLOSE delete_cursor;
    DEALLOCATE delete_cursor;

    -- Re-habilitar y re-chequear constraints.
    DECLARE enable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN @TablesToClear c ON c.TableName = t.name
        WHERE t.is_ms_shipped = 0;

    OPEN enable_cursor;
    FETCH NEXT FROM enable_cursor INTO @SchemaName, @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @QualifiedName = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);
        SET @Sql = N'ALTER TABLE ' + @QualifiedName + N' WITH CHECK CHECK CONSTRAINT ALL;';
        EXEC sys.sp_executesql @Sql;

        FETCH NEXT FROM enable_cursor INTO @SchemaName, @TableName;
    END

    CLOSE enable_cursor;
    DEALLOCATE enable_cursor;

    COMMIT TRAN;
    PRINT N'Datos transaccionales eliminados.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH;
