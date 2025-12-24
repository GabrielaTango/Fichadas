-- =====================================================
-- Script de migración: CONCEPTOS → NOVEDADES
-- =====================================================
-- Este script migra de ba_conceptos a ba_novedades
-- y actualiza la tabla ba_fichadas
--
-- IMPORTANTE:
-- 1. Hacer BACKUP de la base de datos antes de ejecutar
-- 2. Ejecutar en FichadasDB
-- 3. Revisar los mensajes para confirmar que todo se ejecutó correctamente
-- =====================================================

USE FichadasDB;
GO

PRINT '========================================';
PRINT 'INICIO DE MIGRACIÓN: CONCEPTOS → NOVEDADES';
PRINT '========================================';
PRINT '';

-- =====================================================
-- PASO 1: Crear tabla ba_novedades
-- =====================================================
PRINT 'PASO 1: Creando tabla ba_novedades...';

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

-- =====================================================
-- PASO 2: Crear índices en ba_novedades
-- =====================================================
PRINT '';
PRINT 'PASO 2: Creando índices en ba_novedades...';

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

-- =====================================================
-- PASO 3: Migrar datos de ba_conceptos a ba_novedades
-- =====================================================
PRINT '';
PRINT 'PASO 3: Migrando datos de ba_conceptos a ba_novedades...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
BEGIN
    DECLARE @conceptosCount INT;
    SELECT @conceptosCount = COUNT(*) FROM ba_conceptos;

    IF @conceptosCount > 0
    BEGIN
        -- Insertar conceptos como novedades (solo si no existen)
        -- Usamos nro_concepto como cod_novedad y desc_concepto como desc_novedad
        INSERT INTO ba_novedades (cod_novedad, desc_novedad, fecha_creacion)
        SELECT
            CAST(nro_concepto AS NVARCHAR(50)) as cod_novedad,
            desc_concepto,
            fecha_creacion
        FROM ba_conceptos
        WHERE NOT EXISTS (
            SELECT 1 FROM ba_novedades n
            WHERE n.cod_novedad = CAST(ba_conceptos.nro_concepto AS NVARCHAR(50))
        );

        DECLARE @migrados INT = @@ROWCOUNT;
        PRINT '✓ ' + CAST(@migrados AS VARCHAR(10)) + ' conceptos migrados a novedades';

        IF @migrados < @conceptosCount
        BEGIN
            PRINT '⚠ ' + CAST(@conceptosCount - @migrados AS VARCHAR(10)) + ' conceptos ya existían como novedades';
        END
    END
    ELSE
    BEGIN
        PRINT '⚠ No hay datos en ba_conceptos para migrar';
    END
END
ELSE
BEGIN
    PRINT '⚠ La tabla ba_conceptos no existe, saltando migración de datos';
END
GO

-- =====================================================
-- PASO 4: Agregar columna novedad_id a ba_fichadas
-- =====================================================
PRINT '';
PRINT 'PASO 4: Agregando columna novedad_id a ba_fichadas...';

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

-- =====================================================
-- PASO 5: Migrar datos de concepto_id a novedad_id
-- =====================================================
PRINT '';
PRINT 'PASO 5: Migrando datos de concepto_id a novedad_id en ba_fichadas...';

IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'concepto_id'
)
AND EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'novedad_id'
)
AND EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
BEGIN
    -- Migrar concepto_id a novedad_id usando mapeo de nro_concepto
    UPDATE f
    SET f.novedad_id = n.id_novedad
    FROM ba_fichadas f
    INNER JOIN ba_conceptos c ON f.concepto_id = c.id_concepto
    INNER JOIN ba_novedades n ON n.cod_novedad = CAST(c.nro_concepto AS NVARCHAR(50))
    WHERE f.concepto_id IS NOT NULL
    AND f.novedad_id IS NULL;

    DECLARE @fichadasMigradas INT = @@ROWCOUNT;
    PRINT '✓ ' + CAST(@fichadasMigradas AS VARCHAR(10)) + ' fichadas migradas de concepto_id a novedad_id';
END
ELSE
BEGIN
    PRINT '⚠ No se puede migrar: verificar que existan ambas columnas y ba_conceptos';
END
GO

-- =====================================================
-- PASO 6: Eliminar FK de concepto_id
-- =====================================================
PRINT '';
PRINT 'PASO 6: Eliminando foreign key FK_ba_fichadas_ba_conceptos...';

IF EXISTS (
    SELECT * FROM sys.foreign_keys
    WHERE name = 'FK_ba_fichadas_ba_conceptos'
)
BEGIN
    ALTER TABLE ba_fichadas
    DROP CONSTRAINT FK_ba_fichadas_ba_conceptos;

    PRINT '✓ Foreign key FK_ba_fichadas_ba_conceptos eliminada';
END
ELSE
BEGIN
    PRINT '⚠ La foreign key FK_ba_fichadas_ba_conceptos no existe';
END
GO

-- =====================================================
-- PASO 7: Eliminar índice de concepto_id
-- =====================================================
PRINT '';
PRINT 'PASO 7: Eliminando índice IX_ba_fichadas_concepto_id...';

IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_fichadas_concepto_id'
    AND object_id = OBJECT_ID('ba_fichadas')
)
BEGIN
    DROP INDEX IX_ba_fichadas_concepto_id ON ba_fichadas;
    PRINT '✓ Índice IX_ba_fichadas_concepto_id eliminado';
END
ELSE
BEGIN
    PRINT '⚠ El índice IX_ba_fichadas_concepto_id no existe';
END
GO

-- =====================================================
-- PASO 8: Eliminar columna concepto_id
-- =====================================================
PRINT '';
PRINT 'PASO 8: Eliminando columna concepto_id de ba_fichadas...';

IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'concepto_id'
)
BEGIN
    ALTER TABLE ba_fichadas
    DROP COLUMN concepto_id;

    PRINT '✓ Columna concepto_id eliminada de ba_fichadas';
END
ELSE
BEGIN
    PRINT '⚠ La columna concepto_id no existe en ba_fichadas';
END
GO

-- =====================================================
-- PASO 9: Crear FK entre ba_fichadas y ba_novedades
-- =====================================================
PRINT '';
PRINT 'PASO 9: Creando foreign key FK_ba_fichadas_ba_novedades...';

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
GO

-- =====================================================
-- PASO 10: Crear índice en novedad_id
-- =====================================================
PRINT '';
PRINT 'PASO 10: Creando índice IX_ba_fichadas_novedad_id...';

IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ba_fichadas_novedad_id'
    AND object_id = OBJECT_ID('ba_fichadas')
)
BEGIN
    CREATE INDEX IX_ba_fichadas_novedad_id ON ba_fichadas(novedad_id);
    PRINT '✓ Índice IX_ba_fichadas_novedad_id creado';
END
ELSE
BEGIN
    PRINT '⚠ El índice IX_ba_fichadas_novedad_id ya existe';
END
GO

-- =====================================================
-- PASO 11: Renombrar tabla ba_conceptos (BACKUP)
-- =====================================================
PRINT '';
PRINT 'PASO 11: Renombrando tabla ba_conceptos a ba_conceptos_OLD (backup)...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
BEGIN
    EXEC sp_rename 'ba_conceptos', 'ba_conceptos_OLD';
    PRINT '✓ Tabla ba_conceptos renombrada a ba_conceptos_OLD';
    PRINT '  (Puede eliminarla manualmente después de verificar que todo funciona)';
END
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
BEGIN
    PRINT '⚠ La tabla ba_conceptos_OLD ya existe';
    PRINT '  La tabla ba_conceptos NO fue renombrada';
END
ELSE
BEGIN
    PRINT '⚠ La tabla ba_conceptos no existe';
END
GO

-- =====================================================
-- RESUMEN FINAL
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT 'MIGRACIÓN COMPLETADA';
PRINT '========================================';
PRINT '';
PRINT 'Verificación final:';

-- Contar novedades
DECLARE @novedadesCount INT = 0;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_novedades')
    SELECT @novedadesCount = COUNT(*) FROM ba_novedades;
PRINT '  - Novedades en ba_novedades: ' + CAST(@novedadesCount AS VARCHAR(10));

-- Contar fichadas con novedad
DECLARE @fichadasConNovedad INT = 0;
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ba_fichadas') AND name = 'novedad_id')
    SELECT @fichadasConNovedad = COUNT(*) FROM ba_fichadas WHERE novedad_id IS NOT NULL;
PRINT '  - Fichadas con novedad asignada: ' + CAST(@fichadasConNovedad AS VARCHAR(10));

-- Verificar si existe ba_conceptos_OLD
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos_OLD')
    PRINT '  - Tabla ba_conceptos_OLD: ✓ Existe (backup disponible)';
ELSE
    PRINT '  - Tabla ba_conceptos_OLD: ✗ No existe';

PRINT '';
PRINT 'IMPORTANTE:';
PRINT '  1. Verifique que los datos se migraron correctamente';
PRINT '  2. Pruebe la aplicación completamente';
PRINT '  3. Si todo funciona, puede eliminar ba_conceptos_OLD con:';
PRINT '     DROP TABLE ba_conceptos_OLD;';
PRINT '';
PRINT '========================================';
GO
