-- Script para crear tabla de configuración de cálculo de horas
-- Esta tabla almacena las reglas de negocio configurables por sector y temporada

USE FichadasDB;
GO

-- Crear tabla de configuración
CREATE TABLE ba_configuracion_calculo (
    id_configuracion INT IDENTITY(1,1) PRIMARY KEY,
    sector_id INT NOT NULL,
    es_verano BIT NOT NULL,

    -- Horas normales esperadas
    horas_normales INT NOT NULL,

    -- Horas extras oficiales (generalmente 1 hora)
    horas_extras_oficiales INT NOT NULL,

    -- Horas extras adicionales (varía por sector y temporada)
    -- Máquinas: 2h, Expedición verano: 1h, Expedición invierno: 0h, Admin: 0h
    horas_extras_adicionales INT NOT NULL,

    -- Tolerancia de llegada tarde (generalmente 5 minutos)
    tolerancia_minutos INT NOT NULL DEFAULT 5,

    -- Descuento si llega tarde entre 6-30 minutos (generalmente 30 min)
    descuento_tarde_6_30_min INT NOT NULL DEFAULT 30,

    -- Descuento si llega tarde 31+ minutos (generalmente 60 min)
    descuento_tarde_31_mas INT NOT NULL DEFAULT 60,

    -- Horarios de referencia (opcional, para validaciones)
    hora_entrada_esperada TIME NULL,
    hora_salida_esperada TIME NULL,

    -- Solo una configuración puede estar activa por sector/temporada
    activo BIT NOT NULL DEFAULT 1,

    -- Auditoría
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_modificacion DATETIME NULL,

    -- Foreign key
    CONSTRAINT FK_ConfigCalculo_Sector FOREIGN KEY (sector_id)
        REFERENCES ba_sectores(id_sector) ON DELETE CASCADE
);
GO

-- Índices para mejorar performance
CREATE INDEX idx_configuracion_sector ON ba_configuracion_calculo(sector_id);
CREATE INDEX idx_configuracion_activo ON ba_configuracion_calculo(activo);
GO

-- Comentarios en las columnas
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Configuración de reglas de cálculo de horas por sector y temporada',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ba_configuracion_calculo';
GO

PRINT 'Tabla ba_configuracion_calculo creada exitosamente';
GO
