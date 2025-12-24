-- =====================================================
-- Script de verificación: Estado de Novedades
-- =====================================================
-- Este script verifica el estado actual de las tablas
-- relacionadas con novedades y conceptos
--
-- Ejecutar ANTES y DESPUÉS de la migración para comparar
-- =====================================================

USE FichadasDB;
GO

PRINT '========================================';
PRINT 'VERIFICACIÓN DE ESTADO: NOVEDADES';
PRINT '========================================';
PRINT '';

-- =====================================================
-- 1. VERIFICAR EXISTENCIA DE TABLAS
-- =====================================================
PRINT '1. EXISTENCIA DE TABLAS:';
PRINT '   ----------------------';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
    PRINT '   ✓ ba_conceptos: EXISTE'
ELSE
    PRINT '   ✗ ba_conceptos: NO EXISTE';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
    PRINT '   ✓ ba_conceptos_OLD: EXISTE (backup)'
ELSE
    PRINT '   ✗ ba_conceptos_OLD: NO EXISTE';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
    PRINT '   ✓ ba_novedades: EXISTE'
ELSE
    PRINT '   ✗ ba_novedades: NO EXISTE';

PRINT '';

-- =====================================================
-- 2. VERIFICAR COLUMNAS EN ba_fichadas
-- =====================================================
PRINT '2. COLUMNAS EN ba_fichadas:';
PRINT '   -------------------------';

IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'concepto_id'
)
    PRINT '   ✓ concepto_id: EXISTE (columna antigua)'
ELSE
    PRINT '   ✗ concepto_id: NO EXISTE';

IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'novedad_id'
)
    PRINT '   ✓ novedad_id: EXISTE (columna nueva)'
ELSE
    PRINT '   ✗ novedad_id: NO EXISTE';

PRINT '';

-- =====================================================
-- 3. CONTAR REGISTROS
-- =====================================================
PRINT '3. CONTEO DE REGISTROS:';
PRINT '   ---------------------';

DECLARE @conceptosCount INT = 0;
DECLARE @conceptosOldCount INT = 0;
DECLARE @novedadesCount INT = 0;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
    SELECT @conceptosCount = COUNT(*) FROM ba_conceptos;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
    SELECT @conceptosOldCount = COUNT(*) FROM ba_conceptos_OLD;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
    SELECT @novedadesCount = COUNT(*) FROM ba_novedades;

PRINT '   ba_conceptos: ' + CAST(@conceptosCount AS VARCHAR(10)) + ' registros';
PRINT '   ba_conceptos_OLD: ' + CAST(@conceptosOldCount AS VARCHAR(10)) + ' registros';
PRINT '   ba_novedades: ' + CAST(@novedadesCount AS VARCHAR(10)) + ' registros';

PRINT '';

-- =====================================================
-- 4. VERIFICAR FICHADAS CON NOVEDADES/CONCEPTOS
-- =====================================================
PRINT '4. FICHADAS CON NOVEDADES/CONCEPTOS:';
PRINT '   -----------------------------------';

DECLARE @fichadasConConcepto INT = 0;
DECLARE @fichadasConNovedad INT = 0;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ba_fichadas') AND name = 'concepto_id')
    SELECT @fichadasConConcepto = COUNT(*) FROM ba_fichadas WHERE concepto_id IS NOT NULL;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ba_fichadas') AND name = 'novedad_id')
    SELECT @fichadasConNovedad = COUNT(*) FROM ba_fichadas WHERE novedad_id IS NOT NULL;

PRINT '   Fichadas con concepto_id: ' + CAST(@fichadasConConcepto AS VARCHAR(10));
PRINT '   Fichadas con novedad_id: ' + CAST(@fichadasConNovedad AS VARCHAR(10));

PRINT '';

-- =====================================================
-- 5. VERIFICAR FOREIGN KEYS
-- =====================================================
PRINT '5. FOREIGN KEYS:';
PRINT '   --------------';

IF EXISTS (
    SELECT * FROM sys.foreign_keys
    WHERE name = 'FK_ba_fichadas_ba_conceptos'
)
    PRINT '   ✓ FK_ba_fichadas_ba_conceptos: EXISTE (antigua)'
ELSE
    PRINT '   ✗ FK_ba_fichadas_ba_conceptos: NO EXISTE';

IF EXISTS (
    SELECT * FROM sys.foreign_keys
    WHERE name = 'FK_ba_fichadas_ba_novedades'
)
    PRINT '   ✓ FK_ba_fichadas_ba_novedades: EXISTE (nueva)'
ELSE
    PRINT '   ✗ FK_ba_fichadas_ba_novedades: NO EXISTE';

PRINT '';

-- =====================================================
-- 6. VERIFICAR ÍNDICES
-- =====================================================
PRINT '6. ÍNDICES:';
PRINT '   ---------';

IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_fichadas_concepto_id'
    AND object_id = OBJECT_ID('ba_fichadas')
)
    PRINT '   ✓ IX_ba_fichadas_concepto_id: EXISTE (antiguo)'
ELSE
    PRINT '   ✗ IX_ba_fichadas_concepto_id: NO EXISTE';

IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_fichadas_novedad_id'
    AND object_id = OBJECT_ID('ba_fichadas')
)
    PRINT '   ✓ IX_ba_fichadas_novedad_id: EXISTE (nuevo)'
ELSE
    PRINT '   ✗ IX_ba_fichadas_novedad_id: NO EXISTE';

IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_novedades_cod_novedad'
    AND object_id = OBJECT_ID('ba_novedades')
)
    PRINT '   ✓ IX_ba_novedades_cod_novedad: EXISTE'
ELSE
    PRINT '   ✗ IX_ba_novedades_cod_novedad: NO EXISTE';

PRINT '';

-- =====================================================
-- 7. ESTADO DE LA MIGRACIÓN
-- =====================================================
PRINT '7. ESTADO DE LA MIGRACIÓN:';
PRINT '   ------------------------';

DECLARE @migracionCompletada BIT = 0;
DECLARE @migracionPendiente BIT = 0;
DECLARE @instalacionNueva BIT = 0;

-- Migración completada: ba_novedades existe, ba_conceptos no existe o renombrada, novedad_id existe
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ba_fichadas') AND name = 'novedad_id')
BEGIN
    SET @migracionCompletada = 1;
    PRINT '';
    PRINT '   ✅ MIGRACIÓN COMPLETADA';
    PRINT '   -------------------------';
    PRINT '   El sistema ha sido migrado exitosamente a novedades.';

    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
    BEGIN
        PRINT '   Existe backup en ba_conceptos_OLD (' + CAST(@conceptosOldCount AS VARCHAR(10)) + ' registros)';
        PRINT '';
        PRINT '   RECOMENDACIÓN:';
        PRINT '   Si todo funciona correctamente, puede eliminar el backup:';
        PRINT '   DROP TABLE ba_conceptos_OLD;';
    END
END

-- Migración pendiente: ba_conceptos existe, ba_novedades no existe
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
BEGIN
    SET @migracionPendiente = 1;
    PRINT '';
    PRINT '   ⚠️ MIGRACIÓN PENDIENTE';
    PRINT '   ----------------------';
    PRINT '   Tiene datos en ba_conceptos pero aún no ha migrado a novedades.';
    PRINT '';
    PRINT '   ACCIÓN REQUERIDA:';
    PRINT '   1. Hacer BACKUP de la base de datos';
    PRINT '   2. Ejecutar: MigrarConceptosANovedades.sql';
END

-- Instalación nueva: ni ba_conceptos ni ba_novedades existen
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
BEGIN
    SET @instalacionNueva = 1;
    PRINT '';
    PRINT '   ℹ️ INSTALACIÓN NUEVA';
    PRINT '   --------------------';
    PRINT '   No se detectaron tablas de conceptos ni novedades.';
    PRINT '';
    PRINT '   ACCIÓN REQUERIDA:';
    PRINT '   1. Ejecutar: CrearTablaNovedades.sql';
    PRINT '   2. Ejecutar: AgregarNovedadAFichadas.sql';
END

-- Estado intermedio: ambas existen (en proceso de migración)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
BEGIN
    PRINT '';
    PRINT '   ⏳ MIGRACIÓN EN PROGRESO O INCOMPLETA';
    PRINT '   -------------------------------------';
    PRINT '   Existen tanto ba_conceptos como ba_novedades.';
    PRINT '';
    PRINT '   VERIFICACIÓN REQUERIDA:';
    PRINT '   - Revise si la migración se completó correctamente';
    PRINT '   - Si fue exitosa, ba_conceptos debería renombrarse a ba_conceptos_OLD';
    PRINT '   - Puede volver a ejecutar: MigrarConceptosANovedades.sql';
END

PRINT '';

-- =====================================================
-- 8. MUESTRA DE DATOS (si existen)
-- =====================================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades') AND @novedadesCount > 0
BEGIN
    PRINT '8. MUESTRA DE NOVEDADES (primeras 10):';
    PRINT '   ------------------------------------';
    SELECT TOP 10
        id_novedad,
        cod_novedad,
        desc_novedad,
        fecha_creacion
    FROM ba_novedades
    ORDER BY id_novedad;
    PRINT '';
END

PRINT '========================================';
PRINT 'FIN DE VERIFICACIÓN';
PRINT '========================================';
PRINT '';
GO
