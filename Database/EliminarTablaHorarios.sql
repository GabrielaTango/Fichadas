-- Script para eliminar la tabla ba_horarios_turno
-- Esta tabla está siendo reemplazada por ba_configuracion_calculo

USE FichadasDB;
GO

-- Verificar que la tabla exista antes de eliminarla
IF OBJECT_ID('ba_horarios_turno', 'U') IS NOT NULL
BEGIN
    DROP TABLE ba_horarios_turno;
    PRINT 'Tabla ba_horarios_turno eliminada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla ba_horarios_turno no existe';
END
GO
