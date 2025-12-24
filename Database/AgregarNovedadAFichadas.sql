-- Agregar campo novedad_id a la tabla ba_fichadas
-- Ejecutar en FichadasDB
-- Este script es para instalaciones NUEVAS o limpias

USE FichadasDB;
GO

PRINT '========================================';
PRINT 'Agregando novedad_id a ba_fichadas';
PRINT '========================================';
PRINT '';

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'novedad_id'
)
BEGIN
    ALTER TABLE ba_fichadas
    ADD novedad_id INT NULL;

    PRINT '✓ Columna novedad_id agregada a ba_fichadas';
END
ELSE
BEGIN
    PRINT '⚠ La columna novedad_id ya existe en ba_fichadas';
END
GO

-- Crear foreign key si existe la tabla ba_novedades
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.foreign_keys
        WHERE name = 'FK_ba_fichadas_ba_novedades'
    )
    BEGIN
        ALTER TABLE ba_fichadas
        ADD CONSTRAINT FK_ba_fichadas_ba_novedades
        FOREIGN KEY (novedad_id) REFERENCES ba_novedades(id_novedad);

        PRINT '✓ Foreign key FK_ba_fichadas_ba_novedades creada';
    END
    ELSE
    BEGIN
        PRINT '⚠ La foreign key FK_ba_fichadas_ba_novedades ya existe';
    END
END
ELSE
BEGIN
    PRINT '⚠ ADVERTENCIA: La tabla ba_novedades no existe. Ejecute CrearTablaNovedades.sql primero.';
END
GO

-- Crear índice para mejor performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ba_fichadas_novedad_id')
BEGIN
    CREATE INDEX IX_ba_fichadas_novedad_id ON ba_fichadas(novedad_id);
    PRINT '✓ Índice IX_ba_fichadas_novedad_id creado';
END
ELSE
BEGIN
    PRINT '⚠ El índice IX_ba_fichadas_novedad_id ya existe';
END
GO

PRINT '';
PRINT 'Script completado exitosamente';
PRINT '';
GO
