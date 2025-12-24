#!/bin/bash

# Iniciar SQL Server en background
/opt/mssql/bin/sqlservr &
SQLPID=$!

# Esperar a que SQL Server este listo
echo "Esperando a que SQL Server inicie..."
sleep 30

for i in {1..50}; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "SQL Server esta listo!"
        break
    fi
    echo "Intento $i: SQL Server no esta listo aun..."
    sleep 2
done

# Verificar si las bases de datos ya existen
FICHADAS_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'FichadasDB'" | tr -d '[:space:]')
DELTA4_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'DELTA4'" | tr -d '[:space:]')

# Restaurar FichadasDB si no existe
if [ "$FICHADAS_EXISTS" = "0" ] && [ -f /var/opt/mssql/backup/FichadasDB.bak ]; then
    echo "Restaurando FichadasDB..."

    # Obtener nombres logicos
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/FichadasDB.bak'"

    # Restaurar
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "
    RESTORE DATABASE [FichadasDB]
    FROM DISK = '/var/opt/mssql/backup/FichadasDB.bak'
    WITH MOVE 'FichadasDB' TO '/var/opt/mssql/data/FichadasDB.mdf',
         MOVE 'FichadasDB_log' TO '/var/opt/mssql/data/FichadasDB_log.ldf',
         REPLACE"

    echo "FichadasDB restaurada!"
else
    echo "FichadasDB ya existe o no hay backup"
fi

# Restaurar DELTA4 si no existe
if [ "$DELTA4_EXISTS" = "0" ] && [ -f /var/opt/mssql/backup/DELTA4.bak ]; then
    echo "Restaurando DELTA4..."

    # Obtener nombres logicos
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/DELTA4.bak'"

    # Restaurar
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "
    RESTORE DATABASE [DELTA4]
    FROM DISK = '/var/opt/mssql/backup/DELTA4.bak'
    WITH MOVE 'DELTA4' TO '/var/opt/mssql/data/DELTA4.mdf',
         MOVE 'DELTA4_log' TO '/var/opt/mssql/data/DELTA4_log.ldf',
         REPLACE"

    echo "DELTA4 restaurada!"
else
    echo "DELTA4 ya existe o no hay backup"
fi

echo "Bases de datos disponibles:"
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases"

# Mantener SQL Server corriendo
wait $SQLPID
