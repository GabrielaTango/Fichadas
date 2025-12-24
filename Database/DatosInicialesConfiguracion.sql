-- Script para insertar datos iniciales de configuración de cálculo
-- Basado en las reglas de negocio del proyecto

USE FichadasDB;
GO

-- Nota: Ajustar los sector_id según los IDs reales en tu base de datos
-- Este script asume:
-- sector_id 1 = Máquinas
-- sector_id 2 = Expedición
-- sector_id 3 = Administración

-- ============================================
-- SECTOR MÁQUINAS
-- ============================================
-- Turno mañana (06:00-18:00): 9h normales + 1h extra oficial + 2h extras adicionales
-- Turno tarde (18:00-06:00): 9h normales + 1h extra oficial + 2h extras adicionales

-- Máquinas - Verano
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    1, 1,           -- Sector Máquinas, Verano
    9, 1, 2,        -- 9h normales, 1h extra oficial, 2h adicionales
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '06:00', '18:00', -- Turno mañana de referencia
    1               -- Activo
);

-- Máquinas - Invierno (mismas reglas que verano para Máquinas)
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    1, 0,           -- Sector Máquinas, Invierno
    9, 1, 2,        -- 9h normales, 1h extra oficial, 2h adicionales
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '06:00', '18:00', -- Turno mañana de referencia
    1               -- Activo
);

-- ============================================
-- SECTOR EXPEDICIÓN
-- ============================================
-- Verano (07:00-18:00): 9h normales + 1h extra oficial + 1h extra adicional
-- Invierno (08:00-18:00): 9h normales + 1h extra oficial (sin adicionales)

-- Expedición - Verano
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    2, 1,           -- Sector Expedición, Verano
    9, 1, 1,        -- 9h normales, 1h extra oficial, 1h adicional
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '07:00', '18:00', -- Horario verano
    1               -- Activo
);

-- Expedición - Invierno (SIN horas adicionales)
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    2, 0,           -- Sector Expedición, Invierno
    9, 1, 0,        -- 9h normales, 1h extra oficial, 0h adicionales ⚠️
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '08:00', '18:00', -- Horario invierno
    1               -- Activo
);

-- ============================================
-- SECTOR ADMINISTRACIÓN
-- ============================================
-- Horario 08:30-18:00 o 09:00-18:00
-- 10 horas (1 hora es almuerzo, no cuenta como extra)
-- Generalmente NO hacen horas extras

-- Administración - Verano
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    3, 1,           -- Sector Administración, Verano
    9, 0, 0,        -- 9h normales (10h - 1h almuerzo), generalmente sin extras
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '08:30', '18:00', -- Horario común (también puede ser 09:00-18:00)
    1               -- Activo
);

-- Administración - Invierno (mismas reglas)
INSERT INTO ba_configuracion_calculo (
    sector_id, es_verano,
    horas_normales, horas_extras_oficiales, horas_extras_adicionales,
    tolerancia_minutos, descuento_tarde_6_30_min, descuento_tarde_31_mas,
    hora_entrada_esperada, hora_salida_esperada,
    activo
) VALUES (
    3, 0,           -- Sector Administración, Invierno
    9, 0, 0,        -- 9h normales (10h - 1h almuerzo), generalmente sin extras
    5, 30, 60,      -- Tolerancia 5min, descuentos 30min y 60min
    '08:30', '18:00', -- Horario común
    1               -- Activo
);

GO

PRINT 'Datos iniciales de configuración insertados exitosamente';
PRINT '';
PRINT 'IMPORTANTE: Verificar que los sector_id coincidan con tu base de datos';
PRINT 'Ejecutar: SELECT * FROM ba_sectores';
PRINT 'Y ajustar los INSERT si es necesario';
GO
