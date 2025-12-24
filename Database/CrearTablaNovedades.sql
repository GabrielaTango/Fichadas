-- Tabla de novedades (importadas desde Tango)
-- Ejecutar en FichadasDB
-- Este script es para instalaciones NUEVAS o limpias

USE FichadasDB;
GO

PRINT '========================================';
PRINT 'Creando tabla ba_novedades';
PRINT '========================================';
PRINT '';

-- Crear tabla ba_novedades
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
BEGIN
    CREATE TABLE ba_novedades (
        id_novedad INT IDENTITY(1,1) PRIMARY KEY,
        cod_novedad NVARCHAR(50) NOT NULL UNIQUE,
        desc_novedad NVARCHAR(200) NOT NULL,
        fecha_creacion DATETIME DEFAULT GETDATE(),
        fecha_modificacion DATETIME DEFAULT GETDATE()
    );

    PRINT '✓ Tabla ba_novedades creada exitosamente';
END
ELSE
BEGIN
    PRINT '⚠ La tabla ba_novedades ya existe';
END
GO

-- Índices para mejor performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ba_novedades_cod_novedad')
BEGIN
    CREATE INDEX IX_ba_novedades_cod_novedad ON ba_novedades(cod_novedad);
    PRINT '✓ Índice IX_ba_novedades_cod_novedad creado';
END
ELSE
BEGIN
    PRINT '⚠ El índice IX_ba_novedades_cod_novedad ya existe';
END
GO

PRINT '';
PRINT 'Script completado exitosamente';
PRINT '';
GO
