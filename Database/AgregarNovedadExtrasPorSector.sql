-- Script de migración para agregar novedad de horas extras por sector
-- Sistema de Fichadas Colombraro
-- Fecha: 2025-12-14

USE FichadasDB;
GO

-- =============================================================================
-- 1. Agregar campo novedad_extras_id a la tabla ba_sectores
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
    AND name = 'novedad_extras_id'
)
BEGIN
    ALTER TABLE [dbo].[ba_sectores]
    ADD [novedad_extras_id] INT NULL;

    PRINT 'Campo novedad_extras_id agregado a ba_sectores';
END
ELSE
BEGIN
    PRINT 'Campo novedad_extras_id ya existe en ba_sectores';
END
GO

-- =============================================================================
-- 2. Agregar foreign key a ba_novedades
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_sectores_novedades_extras'
)
BEGIN
    ALTER TABLE [dbo].[ba_sectores]
    ADD CONSTRAINT FK_sectores_novedades_extras
    FOREIGN KEY (novedad_extras_id) REFERENCES [dbo].[ba_novedades](id_novedad);

    PRINT 'Foreign key FK_sectores_novedades_extras creado';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_sectores_novedades_extras ya existe';
END
GO

-- =============================================================================
-- 3. Crear índice para mejorar performance
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_sectores_novedad_extras'
    AND object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
)
BEGIN
    CREATE INDEX idx_sectores_novedad_extras ON [dbo].[ba_sectores]([novedad_extras_id]);
    PRINT 'Índice idx_sectores_novedad_extras creado';
END
ELSE
BEGIN
    PRINT 'Índice idx_sectores_novedad_extras ya existe';
END
GO

-- =============================================================================
-- 4. Agregar comentario descriptivo
-- =============================================================================
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'ID de la novedad a usar para exportar horas extras a Tango',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_sectores',
    @level2type = N'COLUMN', @level2name = N'novedad_extras_id';
GO

PRINT '';
PRINT '========================================================';
PRINT 'Migración de novedad extras por sector completada';
PRINT '========================================================';
PRINT '';
PRINT 'Resumen de cambios:';
PRINT '- ba_sectores: campo novedad_extras_id agregado';
PRINT '- Foreign key a ba_novedades creada';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- Cada sector debe configurar qué novedad usar para las horas extras';
PRINT '- Al exportar a Tango:';
PRINT '  * Horas trabajadas: se exportan con la novedad de la fichada';
PRINT '  * Horas extras: se exportan con la novedad configurada del sector';
PRINT '  * Horas adicionales: NO se exportan';
PRINT '';
GO
