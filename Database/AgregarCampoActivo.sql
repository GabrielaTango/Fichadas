-- Script para agregar campo 'activo' a la tabla ba_horarios_turno
-- Solo un horario puede estar activo por sector

USE FichadasDB;
GO

-- Agregar la columna activo (por defecto FALSE)
ALTER TABLE ba_horarios_turno
ADD activo BIT NOT NULL DEFAULT 0;
GO

-- Comentario explicativo
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Indica si este horario está activo. Solo puede haber un horario activo por sector.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_horarios_turno',
    @level2type = N'COLUMN', @level2name = N'activo';
GO

PRINT 'Campo activo agregado exitosamente a ba_horarios_turno';
