-- Script de migración para implementar turnos rotativos
-- Sistema de Fichadas Colombraro
-- Fecha: 2025-12-11

USE FichadasDB;
GO

-- =============================================================================
-- 1. Agregar campo es_rotativo a la tabla ba_sectores
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
    AND name = 'es_rotativo'
)
BEGIN
    ALTER TABLE [dbo].[ba_sectores]
    ADD [es_rotativo] BIT NOT NULL DEFAULT 0;

    PRINT 'Campo es_rotativo agregado a ba_sectores';
END
ELSE
BEGIN
    PRINT 'Campo es_rotativo ya existe en ba_sectores';
END
GO

-- =============================================================================
-- 2. Agregar campo fecha_inicio_rotacion a la tabla ba_empleados
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ba_empleados]')
    AND name = 'fecha_inicio_rotacion'
)
BEGIN
    ALTER TABLE [dbo].[ba_empleados]
    ADD [fecha_inicio_rotacion] DATE NULL;

    PRINT 'Campo fecha_inicio_rotacion agregado a ba_empleados';
END
ELSE
BEGIN
    PRINT 'Campo fecha_inicio_rotacion ya existe en ba_empleados';
END
GO

-- =============================================================================
-- 3. Agregar campo tipo_turno a la tabla ba_configuracion_calculo
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ba_configuracion_calculo]')
    AND name = 'tipo_turno'
)
BEGIN
    ALTER TABLE [dbo].[ba_configuracion_calculo]
    ADD [tipo_turno] NVARCHAR(10) NULL;

    -- Valores permitidos: 'diurno', 'nocturno', NULL (para sectores no rotativos)
    ALTER TABLE [dbo].[ba_configuracion_calculo]
    ADD CONSTRAINT CHK_tipo_turno CHECK (tipo_turno IN ('diurno', 'nocturno') OR tipo_turno IS NULL);

    PRINT 'Campo tipo_turno agregado a ba_configuracion_calculo con constraint';
END
ELSE
BEGIN
    PRINT 'Campo tipo_turno ya existe en ba_configuracion_calculo';
END
GO

-- =============================================================================
-- 4. Crear índices para mejorar performance
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_sectores_es_rotativo'
    AND object_id = OBJECT_ID(N'[dbo].[ba_sectores]')
)
BEGIN
    CREATE INDEX idx_sectores_es_rotativo ON [dbo].[ba_sectores]([es_rotativo]);
    PRINT 'Índice idx_sectores_es_rotativo creado';
END
ELSE
BEGIN
    PRINT 'Índice idx_sectores_es_rotativo ya existe';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_empleados_fecha_inicio_rotacion'
    AND object_id = OBJECT_ID(N'[dbo].[ba_empleados]')
)
BEGIN
    CREATE INDEX idx_empleados_fecha_inicio_rotacion ON [dbo].[ba_empleados]([fecha_inicio_rotacion]);
    PRINT 'Índice idx_empleados_fecha_inicio_rotacion creado';
END
ELSE
BEGIN
    PRINT 'Índice idx_empleados_fecha_inicio_rotacion ya existe';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_configuracion_tipo_turno'
    AND object_id = OBJECT_ID(N'[dbo].[ba_configuracion_calculo]')
)
BEGIN
    CREATE INDEX idx_configuracion_tipo_turno ON [dbo].[ba_configuracion_calculo]([tipo_turno]);
    PRINT 'Índice idx_configuracion_tipo_turno creado';
END
ELSE
BEGIN
    PRINT 'Índice idx_configuracion_tipo_turno ya existe';
END
GO

-- =============================================================================
-- 5. Agregar comentarios descriptivos
-- =============================================================================
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Indica si el sector tiene turnos rotativos (semanal: día/noche)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_sectores',
    @level2type = N'COLUMN', @level2name = N'es_rotativo';
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Fecha de inicio de la rotación semanal (solo para empleados en sectores rotativos)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_empleados',
    @level2type = N'COLUMN', @level2name = N'fecha_inicio_rotacion';
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Tipo de turno para sectores rotativos: diurno (antes 12 PM) o nocturno (después 12 PM)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_configuracion_calculo',
    @level2type = N'COLUMN', @level2name = N'tipo_turno';
GO

PRINT '';
PRINT '========================================================';
PRINT 'Migración de turnos rotativos completada exitosamente';
PRINT '========================================================';
PRINT '';
PRINT 'Resumen de cambios:';
PRINT '- ba_sectores: campo es_rotativo agregado';
PRINT '- ba_empleados: campo fecha_inicio_rotacion agregado';
PRINT '- ba_configuracion_calculo: campo tipo_turno agregado';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- Los sectores rotativos pueden tener 2 configuraciones activas:';
PRINT '  una con tipo_turno = ''diurno'' y otra con tipo_turno = ''nocturno''';
PRINT '- La fecha_inicio_rotacion se usa para calcular qué turno corresponde';
PRINT '- Cálculo: (fecha_fichada - fecha_inicio_rotacion) / 7';
PRINT '  * Resultado impar = turno diurno';
PRINT '  * Resultado par = turno nocturno';
PRINT '';
GO
