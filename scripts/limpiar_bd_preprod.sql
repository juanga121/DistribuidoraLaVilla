-- Pre-production cleanup script for SQL Server
-- Keeps only:
--   - estados
--   - estados_factura
--   - tipos_factura
--   - TiposMovimientoMateriaPrima
--   - unidad_medida
--
-- It deletes all rows from every other user table, then reseeds identity columns.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @PreservedTables TABLE (
        TableName sysname NOT NULL PRIMARY KEY
    );

    INSERT INTO @PreservedTables (TableName)
    VALUES
        ('estados'),
        ('estados_factura'),
        ('tipos_factura'),
        ('TiposMovimientoMateriaPrima'),
        ('unidad_medida'),
        ('__EFMigrationsHistory');

    DECLARE @SchemaName sysname;
    DECLARE @TableName sysname;
    DECLARE @QualifiedName nvarchar(300);
    DECLARE @Sql nvarchar(max);

    -- Disable constraints on all user tables first so deletes do not fail due to FK order.
    DECLARE disable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
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

    -- Delete everything except the preserved tables and migration history.
    DECLARE delete_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        LEFT JOIN @PreservedTables p ON p.TableName = t.name
        WHERE t.is_ms_shipped = 0
          AND p.TableName IS NULL;

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

    -- Re-enable and re-check all constraints.
    DECLARE enable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH;
