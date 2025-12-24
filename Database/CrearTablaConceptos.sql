-- Tabla de conceptos de novedades (importados desde Tango)
-- Ejecutar en FichadasDB

USE FichadasDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ba_conceptos')
BEGIN
    CREATE TABLE ba_conceptos (
        id_concepto INT IDENTITY(1,1) PRIMARY KEY,
        id_concepto_tango INT NOT NULL UNIQUE,
        nro_concepto INT NOT NULL,
        desc_concepto NVARCHAR(200) NOT NULL,
        fecha_creacion DATETIME DEFAULT GETDATE(),
        fecha_modificacion DATETIME DEFAULT GETDATE()
    );

    PRINT 'Tabla ba_conceptos creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla ba_conceptos ya existe';
END
GO

-- Índices para mejor performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ba_conceptos_id_concepto_tango')
BEGIN
    CREATE INDEX IX_ba_conceptos_id_concepto_tango ON ba_conceptos(id_concepto_tango);
    PRINT 'Índice IX_ba_conceptos_id_concepto_tango creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ba_conceptos_nro_concepto')
BEGIN
    CREATE INDEX IX_ba_conceptos_nro_concepto ON ba_conceptos(nro_concepto);
    PRINT 'Índice IX_ba_conceptos_nro_concepto creado';
END
GO

PRINT 'Script de creación de tabla ba_conceptos completado';
