-- Script de creación de base de datos para Sistema de Fichadas Colombraro
-- SQL Server

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FichadasDB')
BEGIN
    CREATE DATABASE FichadasDB;
END
GO

USE FichadasDB;
GO
