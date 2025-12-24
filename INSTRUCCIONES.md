# Instrucciones de Instalación - Sistema de Fichadas Colombraro

## Backend (ASP.NET Core 8 Web API)

### 1. Crear la solución y proyecto

```bash
# En el directorio raíz del repositorio
dotnet new sln -n FichadasSystem

# Crear el proyecto Web API
dotnet new webapi -n FichadasAPI -o Backend/FichadasAPI

# Agregar el proyecto a la solución
dotnet sln add Backend/FichadasAPI/FichadasAPI.csproj
```

### 2. Instalar paquetes NuGet

```bash
cd Backend/FichadasAPI

dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
dotnet add package BCrypt.Net-Next
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package EPPlus
```

### 3. Configurar la base de datos

1. Abrir SQL Server Management Studio
2. Ejecutar el script `Database/CreacionSQLServer.sql`
3. Actualizar el connection string en `Backend/FichadasAPI/appsettings.json` con tus credenciales

### 4. Ejecutar el proyecto

```bash
cd Backend/FichadasAPI
dotnet run
```

La API estará disponible en `https://localhost:5001` o `http://localhost:5000`

## Frontend (React + TypeScript)

### Instrucciones para crear el proyecto frontend

```bash
# Opción 1: Usando Vite (recomendado)
npm create vite@latest Frontend -- --template react-ts

# Opción 2: Usando Create React App
npx create-react-app Frontend --template typescript

cd Frontend
npm install
```

### Paquetes adicionales necesarios

```bash
npm install axios
npm install react-router-dom
npm install @types/react-router-dom
npm install react-hook-form
npm install @tanstack/react-query
npm install lucide-react
```

## Estructura del Proyecto

```
Colombraro Fichadas/
├── Database/
│   ├── creacion.sql (MySQL original)
│   └── CreacionSQLServer.sql (Convertido a SQL Server)
├── Backend/
│   └── FichadasAPI/
│       ├── Controllers/
│       ├── Models/
│       ├── Repositories/
│       ├── Services/
│       ├── Data/
│       └── Program.cs
├── Frontend/
│   └── (Por crear)
└── Proyecto.txt
```

## Endpoints de la API

### Autenticación
- `POST /api/auth/login` - Login de usuario
- `POST /api/auth/change-password` - Cambiar contraseña

### Sectores
- `GET /api/sectores` - Obtener todos los sectores
- `GET /api/sectores/{id}` - Obtener sector por ID
- `POST /api/sectores` - Crear sector (Admin)
- `PUT /api/sectores/{id}` - Actualizar sector (Admin)
- `DELETE /api/sectores/{id}` - Eliminar sector (Admin)

### Empleados
- `GET /api/empleados` - Obtener todos los empleados
- `GET /api/empleados/{id}` - Obtener empleado por ID
- `GET /api/empleados/legajo/{legajo}` - Obtener empleado por legajo
- `GET /api/empleados/sector/{sectorId}` - Obtener empleados por sector
- `POST /api/empleados` - Crear empleado (Admin)
- `PUT /api/empleados/{id}` - Actualizar empleado (Admin)
- `DELETE /api/empleados/{id}` - Eliminar empleado (Admin)

### Fichadas
- `GET /api/fichadas` - Obtener todas las fichadas
- `GET /api/fichadas/{id}` - Obtener fichada por ID
- `GET /api/fichadas/empleado/{empleadoId}` - Obtener fichadas por empleado
- `GET /api/fichadas/rango?fechaDesde=&fechaHasta=` - Obtener fichadas por rango de fechas
- `POST /api/fichadas` - Crear fichada
- `POST /api/fichadas/importar` - Importar fichadas desde Excel
- `PUT /api/fichadas/{id}` - Actualizar fichada
- `DELETE /api/fichadas/{id}` - Eliminar fichada (Admin)

## Notas Importantes

1. Cambiar el `SecretKey` en `appsettings.json` antes de producción
2. Configurar correctamente los connection strings
3. La importación desde Excel requiere implementación adicional del servicio
4. El cálculo de horas normales, extras y adicionales está pendiente de implementación según las reglas del negocio
