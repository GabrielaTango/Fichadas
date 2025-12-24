-- Script de migración para agregar novedad por defecto de horas trabajadas por sector
-- Sistema de Fichadas Colombraro
-- Fecha: 2025-12-14

USE FichadasDB;
GO

-- =============================================================================
-- 1. Agregar campo novedad_trabajadas_id a la tabla ba_sectores
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
    AND name = 'novedad_trabajadas_id'
)
BEGIN
    ALTER TABLE [dbo].[ba_sectores]
    ADD [novedad_trabajadas_id] INT NULL;

    PRINT 'Campo novedad_trabajadas_id agregado a ba_sectores';
END
ELSE
BEGIN
    PRINT 'Campo novedad_trabajadas_id ya existe en ba_sectores';
END
GO

-- =============================================================================
-- 2. Agregar foreign key a ba_novedades
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_sectores_novedades_trabajadas'
)
BEGIN
    ALTER TABLE [dbo].[ba_sectores]
    ADD CONSTRAINT FK_sectores_novedades_trabajadas
    FOREIGN KEY (novedad_trabajadas_id) REFERENCES [dbo].[ba_novedades](id_novedad);

    PRINT 'Foreign key FK_sectores_novedades_trabajadas creado';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_sectores_novedades_trabajadas ya existe';
END
GO

-- =============================================================================
-- 3. Crear índice para mejorar performance
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_sectores_novedad_trabajadas'
    AND object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
)
BEGIN
    CREATE INDEX idx_sectores_novedad_trabajadas ON [dbo].[ba_sectores]([novedad_trabajadas_id]);
    PRINT 'Índice idx_sectores_novedad_trabajadas creado';
END
ELSE
BEGIN
    PRINT 'Índice idx_sectores_novedad_trabajadas ya existe';
END
GO

-- =============================================================================
-- 4. Agregar comentario descriptivo
-- =============================================================================
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'ID de la novedad por defecto a usar para horas trabajadas al importar fichadas desde Excel',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_sectores',
    @level2type = N'COLUMN', @level2name = N'novedad_trabajadas_id';
GO

PRINT '';
PRINT '========================================================';
PRINT 'Migración de novedad trabajadas por sector completada';
PRINT '========================================================';
PRINT '';
PRINT 'Resumen de cambios:';
PRINT '- ba_sectores: campo novedad_trabajadas_id agregado';
PRINT '- Foreign key a ba_novedades creada';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- Cada sector puede configurar una novedad por defecto para horas trabajadas';
PRINT '- Al importar fichadas desde Excel:';
PRINT '  * Si el sector tiene novedad_trabajadas_id configurada, se usa esa';
PRINT '  * Si no, se puede requerir que cada fichada especifique su novedad';
PRINT '';
GO
