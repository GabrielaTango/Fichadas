-- Agregar campo exportada a la tabla ba_fichadas
-- Este campo indica si la fichada fue exportada a Tango (NOVEDAD_REGISTRADA)
-- Ejecutar en FichadasDB

USE FichadasDB;
GO

PRINT '========================================';
PRINT 'Agregando campo exportada a ba_fichadas';
PRINT '========================================';
PRINT '';

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'exportada'
)
BEGIN
    ALTER TABLE ba_fichadas
    ADD exportada BIT NOT NULL DEFAULT 0;

    PRINT '✓ Columna exportada agregada a ba_fichadas';
    PRINT '  Default: 0 (no exportada)';
END
ELSE
BEGIN
    PRINT '⚠ La columna exportada ya existe en ba_fichadas';
END
GO

-- Crear índice para mejor performance en consultas de fichadas no exportadas
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_fichadas_exportada'
    AND object_id = OBJECT_ID('ba_fichadas')
)
BEGIN
    CREATE INDEX IX_ba_fichadas_exportada ON ba_fichadas(exportada);
    PRINT '✓ Índice IX_ba_fichadas_exportada creado';
END
ELSE
BEGIN
    PRINT '⚠ El índice IX_ba_fichadas_exportada ya existe';
END
GO

-- Agregar campos para rastrear la exportación (opcional pero útil)
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'fecha_exportacion'
)
BEGIN
    ALTER TABLE ba_fichadas
    ADD fecha_exportacion DATETIME NULL;

    PRINT '✓ Columna fecha_exportacion agregada a ba_fichadas';
END
ELSE
BEGIN
    PRINT '⚠ La columna fecha_exportacion ya existe en ba_fichadas';
END
GO

-- Mostrar estadísticas
DECLARE @totalFichadas INT;
DECLARE @fichadasExportadas INT;
DECLARE @fichadasNoExportadas INT;

SELECT @totalFichadas = COUNT(*) FROM ba_fichadas;
SELECT @fichadasExportadas = COUNT(*) FROM ba_fichadas WHERE exportada = 1;
SELECT @fichadasNoExportadas = COUNT(*) FROM ba_fichadas WHERE exportada = 0;

PRINT '';
PRINT 'ESTADÍSTICAS:';
PRINT '  Total fichadas: ' + CAST(@totalFichadas AS VARCHAR(10));
PRINT '  Exportadas: ' + CAST(@fichadasExportadas AS VARCHAR(10));
PRINT '  No exportadas: ' + CAST(@fichadasNoExportadas AS VARCHAR(10));
PRINT '';
PRINT 'Script completado exitosamente';
PRINT '';
GO
