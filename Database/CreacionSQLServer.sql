-- SQL Server Script - Conversión desde MySQL
-- Sistema de Fichadas Colombraro
-- Generado: 2025-11-16

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FichadasDB')
BEGIN
    CREATE DATABASE FichadasDB;
END
GO

USE FichadasDB;
GO

-- -----------------------------------------------------
-- Tabla ba_sectores
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_sectores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_sectores] (
        [id_sector] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [nombre] NVARCHAR(45) NULL
    );
END
GO

-- -----------------------------------------------------
-- Tabla ba_horarios_turno
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_horarios_turno]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_horarios_turno] (
        [id_horarios_turno] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [turno_id] INT NULL,
        [hora_entrada] TIME NULL,
        [hora_salida] TIME NULL,
        [es_verano] BIT NULL,
        [descripcion] NVARCHAR(45) NULL,
        [sector_id] INT NOT NULL,
        CONSTRAINT [fk_ba_horarios_turno_ba_sectores1]
            FOREIGN KEY ([sector_id])
            REFERENCES [dbo].[ba_sectores] ([id_sector])
    );

    CREATE INDEX [idx_horarios_turno_sector] ON [dbo].[ba_horarios_turno] ([sector_id]);
END
GO

-- -----------------------------------------------------
-- Tabla ba_usuarios
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_usuarios] (
        [id_usuario] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [usuario] NVARCHAR(45) NULL,
        [password] NVARCHAR(255) NULL,  -- Aumentado para hash de contraseña
        [mail] NVARCHAR(45) NULL,
        [es_admin] BIT NULL
    );
END
GO

-- -----------------------------------------------------
-- Tabla ba_auditoria
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_auditoria]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_auditoria] (
        [id_auditoria] INT NOT  IDENTITY(1,1) NULL PRIMARY KEY,
        [accion] NVARCHAR(20) NULL,
        [usuario_id] INT NULL,
        [fecha_hora] DATETIME NULL,
        CONSTRAINT [fk_ba_auditoria_ba_usuarios1]
            FOREIGN KEY ([usuario_id])
            REFERENCES [dbo].[ba_usuarios] ([id_usuario])
    );

    CREATE INDEX [idx_auditoria_usuario] ON [dbo].[ba_auditoria] ([usuario_id]);
END
GO

-- -----------------------------------------------------
-- Tabla ba_empleados
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_empleados]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_empleados] (
        [id_empleado] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [nombre] NVARCHAR(45) NULL,
        [legajo] INT NULL,
        [sector_id] INT NULL,
        CONSTRAINT [fk_ba_empleados_ba_sectores1]
            FOREIGN KEY ([sector_id])
            REFERENCES [dbo].[ba_sectores] ([id_sector])
    );

    CREATE INDEX [idx_empleados_sector] ON [dbo].[ba_empleados] ([sector_id]);
    CREATE INDEX [idx_empleados_legajo] ON [dbo].[ba_empleados] ([legajo]);
END
GO

-- -----------------------------------------------------
-- Tabla ba_fichadas
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ba_fichadas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ba_fichadas] (
        [id_fichadas] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [empleado_id] INT NULL,
        [hora_entrada] DATETIME NULL,
        [hora_salida] DATETIME NULL,
        [horario_turno_id] INT NULL,
        [horas_totales] INT NULL,
        [trabajadas] INT NULL,
        [extras] INT NULL,
        [adicionales] INT NULL,
        [codigo_novedad] NVARCHAR(15) NULL,
        CONSTRAINT [fk_ba_fichadas_ba_horarios_turno1]
            FOREIGN KEY ([horario_turno_id])
            REFERENCES [dbo].[ba_horarios_turno] ([id_horarios_turno]),
        CONSTRAINT [fk_ba_fichadas_ba_empleados1]
            FOREIGN KEY ([empleado_id])
            REFERENCES [dbo].[ba_empleados] ([id_empleado])
    );

    CREATE INDEX [idx_fichadas_horario_turno] ON [dbo].[ba_fichadas] ([horario_turno_id]);
    CREATE INDEX [idx_fichadas_empleado] ON [dbo].[ba_fichadas] ([empleado_id]);
    CREATE INDEX [idx_fichadas_fecha_entrada] ON [dbo].[ba_fichadas] ([hora_entrada]);
END
GO

PRINT 'Base de datos creada exitosamente';
GO
