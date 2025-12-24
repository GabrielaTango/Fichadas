-- Agregar campo concepto_id a la tabla ba_fichadas
-- Ejecutar en FichadasDB

USE FichadasDB;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('ba_fichadas')
    AND name = 'concepto_id'
)
BEGIN
    ALTER TABLE ba_fichadas
    ADD concepto_id INT NULL;

    PRINT 'Columna concepto_id agregada a ba_fichadas';
END
ELSE
BEGIN
    PRINT 'La columna concepto_id ya existe en ba_fichadas';
END
GO

-- Crear foreign key si existe la tabla ba_conceptos
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.foreign_keys
        WHERE name = 'FK_ba_fichadas_ba_conceptos'
    )
    BEGIN
        ALTER TABLE ba_fichadas
        ADD CONSTRAINT FK_ba_fichadas_ba_conceptos
        FOREIGN KEY (concepto_id) REFERENCES ba_conceptos(id_concepto);

        PRINT 'Foreign key FK_ba_fichadas_ba_conceptos creada';
    END
    ELSE
    BEGIN
        PRINT 'La foreign key FK_ba_fichadas_ba_conceptos ya existe';
    END
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: La tabla ba_conceptos no existe. Ejecute CrearTablaConceptos.sql primero.';
END
GO

-- Crear índice para mejor performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ba_fichadas_concepto_id')
BEGIN
    CREATE INDEX IX_ba_fichadas_concepto_id ON ba_fichadas(concepto_id);
    PRINT 'Índice IX_ba_fichadas_concepto_id creado';
END
ELSE
BEGIN
    PRINT 'El índice IX_ba_fichadas_concepto_id ya existe';
END
GO

PRINT 'Script completado exitosamente';
