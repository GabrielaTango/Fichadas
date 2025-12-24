# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Descripción del Proyecto

Sistema de administración de control de personal (Fichadas y Licencias) para Colombraro. Permite importar fichadas desde Excel, calcular horas normales/extras/adicionales, gestionar novedades y licencias, y generar reportes para RRHH.

## Tecnologías

- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: React 19 con TypeScript (Vite)
- **Base de Datos**: SQL Server
- **ORM**: Dapper
- **Autenticación**: JWT Bearer
- **UI**: Bootstrap 5
- **Estado**: Zustand (auth) y TanStack Query (server state)

## Comandos Principales

### Backend

```bash
# Restaurar paquetes
cd Backend/FichadasAPI
dotnet restore

# Compilar el proyecto
dotnet build

# Ejecutar la API (default: http://localhost:5210)
dotnet run

# Ejecutar con hot reload
dotnet watch run

# Compilar toda la solución
cd Backend
dotnet build
```

La API estará disponible en:
- HTTP: `http://localhost:5210`
- Swagger UI: `http://localhost:5210/swagger`

### Frontend

```bash
cd Frontend

# Instalar dependencias
npm install

# Ejecutar en desarrollo (default: http://localhost:5173)
npm run dev

# Compilar para producción
npm run build

# Preview de producción
npm run preview

# Linting
npm run lint
```

### Base de Datos

Scripts en `Database/`:
- `CreacionSQLServer.sql`: Script principal de creación de base de datos y tablas
- `01_CreateDatabase.sql`: Creación de base de datos
- `CrearTablaConfiguracionCalculo.sql`: Tabla de configuración de cálculo de horas
- `DatosInicialesConfiguracion.sql`: Datos iniciales de configuración
- Scripts de migración adicionales disponibles en el directorio

Ejecutar scripts en SQL Server Management Studio en el orden indicado por el nombre del archivo.

## Arquitectura del Backend

### Estructura de Carpetas

```
Backend/FichadasAPI/
├── Controllers/                        # Endpoints de la API
│   ├── AuthController.cs              # Login y cambio de contraseña
│   ├── SectoresController.cs          # ABM de sectores
│   ├── EmpleadosController.cs         # ABM de empleados
│   ├── FichadasController.cs          # ABM y consulta de fichadas
│   └── ConfiguracionCalculoController.cs  # Config de cálculo de horas
├── Models/                            # DTOs y entidades
├── Repositories/                      # Capa de acceso a datos con Dapper
│   ├── I{Entidad}Repository.cs       # Interfaces
│   └── {Entidad}Repository.cs        # Implementaciones
├── Services/                          # Lógica de negocio
│   ├── IAuthService.cs / AuthService.cs
│   ├── IHorasCalculoService.cs / HorasCalculoService.cs
│   └── IFichadaImportService.cs / FichadaImportService.cs
├── Data/                              # Contexto de Dapper
│   └── DapperContext.cs              # Crea conexiones a FichadasDB y TangoDB
└── Program.cs                         # Configuración, DI, JWT, CORS, Swagger
```

### Patrón Repository + Service

El proyecto usa una arquitectura en capas:
- **Controllers**: Reciben requests HTTP, validan input, llaman a Services
- **Services**: Contienen lógica de negocio compleja (cálculo de horas, importación)
- **Repositories**: Acceso a datos con Dapper, ejecutan queries SQL
- **Models**: DTOs para transferencia de datos y entidades del dominio

Todos los repositorios y servicios se inyectan como Scoped en el DI container (Program.cs:19-28).

### Autenticación

- JWT Bearer con roles (Admin/User)
- Contraseñas hasheadas con BCrypt
- Secret key en `appsettings.json` (cambiar en producción)

### CORS

Configurado en `Program.cs:54-64` para permitir peticiones desde cualquier origen (AllowAnyOrigin).
En producción, cambiar a orígenes específicos usando `.WithOrigins()`.

## Arquitectura del Frontend

### Estructura de Carpetas

```
Frontend/src/
├── components/          # Componentes reutilizables
│   ├── Layout.tsx      # Layout con Navbar
│   └── Navbar.tsx      # Barra de navegación
├── pages/              # Páginas de la aplicación
│   ├── Login.tsx       # Login de usuarios
│   ├── Home.tsx        # Dashboard principal
│   ├── Empleados.tsx   # ABM de empleados
│   ├── Sectores.tsx    # ABM de sectores
│   ├── Fichadas.tsx    # Consulta de fichadas
│   ├── ImportarFichadas.tsx  # Importación de Excel
│   └── Configuraciones.tsx   # Config de cálculo
├── services/           # Servicios API (axios)
│   ├── api.ts          # Cliente axios con interceptores
│   ├── authService.ts  # Login, logout, getCurrentUser
│   ├── empleadosService.ts
│   ├── sectoresService.ts
│   ├── fichadasService.ts
│   └── configuracionesService.ts
├── stores/             # Estado global con Zustand
│   └── authStore.ts    # Estado de autenticación
├── types/              # Tipos TypeScript
├── hooks/              # Custom hooks
├── utils/              # Utilidades
├── App.tsx             # Router y QueryClientProvider
└── main.tsx            # Entry point, Bootstrap imports
```

### Stack de Estado

- **Zustand**: Para estado de autenticación (authStore)
- **TanStack Query**: Para server state, cache y mutaciones
- **Local Storage**: Para persistir token JWT

El frontend hace fetch de datos con axios + react-query. El token JWT se almacena en localStorage y se envía automáticamente en headers (services/api.ts).

### Rutas Principales

- `/login` - Login público
- `/` - Dashboard (requiere auth)
- `/empleados` - ABM Empleados
- `/sectores` - ABM Sectores
- `/fichadas` - Consulta de fichadas
- `/importar-fichadas` - Importación desde Excel
- `/configuraciones` - Configuración de cálculo de horas

Todas las rutas (excepto `/login`) requieren autenticación. El Layout wrappea las rutas autenticadas.

## Modelo de Base de Datos

### Tablas Principales

1. **ba_usuarios**: Usuarios del sistema (RRHH)
2. **ba_sectores**: Sectores (Máquinas, Expedición, Administración)
3. **ba_empleados**: Empleados (vinculados con legajos de Tango)
4. **ba_configuracion_calculo**: Configuración de reglas de cálculo de horas por sector
5. **ba_fichadas**: Registros de entrada/salida con horas calculadas
6. **ba_auditoria**: Registro de acciones (pendiente de implementación completa)

### Nomenclatura

- IDs con `IDENTITY(1,1)`
- Campos `NVARCHAR` para Unicode
- Prefijo `ba_` en todas las tablas
- Nomenclatura snake_case en BD, PascalCase en C#

## Reglas de Negocio Importantes

### Cálculo de Horas

El cálculo de horas se realiza en `HorasCalculoService.cs` basándose en la configuración de `ba_configuracion_calculo`.

**Proceso de cálculo:**
1. Obtener el empleado y su sector
2. Obtener la configuración activa del sector
3. Calcular total de minutos trabajados (salida - entrada)
4. Aplicar descuento por llegada tarde (si aplica)
5. Calcular horas normales, extras oficiales y adicionales según configuración
6. Retornar `ResultadoCalculoHoras` con desglose completo

**Configuración por Sector (definida en `ba_configuracion_calculo`):**

**Empleados de Máquinas:**
- Turno mañana (06:00-18:00): 9h normales + 1h extra oficial + 2h extras adicionales
- Turno tarde (18:00-06:00): 9h normales + 1h extra oficial + 2h extras adicionales
- Total: 12 horas trabajadas

**Empleados de Expedición:**
- Verano (07:00-18:00): 9h normales + 1h extra oficial + 1h extra adicional
- Invierno (08:00-18:00): 9h normales + 1h extra oficial (sin adicionales)

**Empleados de Administración:**
- Horario 08:30-18:00 o 09:00-18:00
- 10 horas (1 hora es almuerzo, no cuenta como extra)
- Generalmente no hacen horas extras

### Tolerancia de Llegadas Tarde

Configurado por sector en `ba_configuracion_calculo`:
- **Tolerancia**: 5 minutos (configurable)
- **6-30 minutos tarde**: descuento de 30 minutos (configurable)
- **31+ minutos tarde**: descuento de 1 hora (configurable)
- **Entrada anticipada**: NO cuenta como extra (excepto choferes con casos especiales)

### Planillas

- **Planilla Oficial**: Horas normales y extras oficiales
- **Planilla Adicional**: Horas adicionales (solo visible si el usuario lo solicita)

## Integración con Tango

Los datos de legajos vienen de la base de datos de Tango (externa). La conexión está configurada en `appsettings.json` como `TangoDB`. El sistema lee los empleados de Tango pero mantiene su propia tabla `ba_empleados` para asociar sector y categoría.

El `DapperContext` (Data/DapperContext.cs) provee dos métodos:
- `CreateConnection()`: Para FichadasDB
- `CreateTangoConnection()`: Para TangoDB

## Importación de Fichadas

La importación desde Excel se realiza en `FichadaImportService.cs`:

**Formato del archivo Excel esperado:**
- Columna A (1): `idPersona` (legajo del empleado)
- Columna G (7): `fecha` (formato yyyyMMdd)
- Columna H (8): `fichadas` (horas separadas por `;`, ej: "06:00;18:00")

**Proceso de importación:**
1. Leer el archivo Excel con EPPlus (configurado como NonCommercial)
2. Validar formato y datos (empezando desde fila 2)
3. Verificar que los empleados existan en `ba_empleados`
4. Parsear las horas de entrada y salida
5. Llamar a `HorasCalculoService` para calcular horas normales/extras/adicionales
6. Insertar fichadas en `ba_fichadas` con los resultados del cálculo
7. Retornar `FichadaImportResult` con contadores de éxito/errores/ignoradas

El servicio maneja errores por fila y retorna un resultado detallado con todas las advertencias.

## Endpoints de la API

Todos los endpoints (excepto `/api/auth/login`) requieren autenticación JWT.

### Autenticación (Público)
- `POST /api/auth/login` - Login de usuario
- `POST /api/auth/change-password` - Cambiar contraseña (requiere auth)

### Sectores (GET: User/Admin | POST/PUT/DELETE: Admin)
- `GET /api/sectores` - Obtener todos los sectores
- `GET /api/sectores/{id}` - Obtener sector por ID
- `POST /api/sectores` - Crear sector
- `PUT /api/sectores/{id}` - Actualizar sector
- `DELETE /api/sectores/{id}` - Eliminar sector

### Empleados (GET: User/Admin | POST/PUT/DELETE: Admin)
- `GET /api/empleados` - Obtener todos los empleados
- `GET /api/empleados/{id}` - Obtener empleado por ID
- `GET /api/empleados/legajo/{legajo}` - Obtener empleado por legajo
- `GET /api/empleados/sector/{sectorId}` - Obtener empleados por sector
- `POST /api/empleados` - Crear empleado
- `PUT /api/empleados/{id}` - Actualizar empleado
- `DELETE /api/empleados/{id}` - Eliminar empleado

### Fichadas (GET/POST/PUT: User/Admin | DELETE: Admin)
- `GET /api/fichadas` - Obtener todas las fichadas
- `GET /api/fichadas/{id}` - Obtener fichada por ID
- `GET /api/fichadas/empleado/{empleadoId}` - Obtener fichadas por empleado
- `GET /api/fichadas/rango?fechaDesde=&fechaHasta=` - Obtener fichadas por rango
- `POST /api/fichadas` - Crear fichada manual
- `POST /api/fichadas/importar` - Importar fichadas desde Excel
- `PUT /api/fichadas/{id}` - Actualizar fichada
- `DELETE /api/fichadas/{id}` - Eliminar fichada

### Configuración de Cálculo (GET: User/Admin | POST/PUT/DELETE: Admin)
- `GET /api/configuracioncalculo` - Obtener todas las configuraciones
- `GET /api/configuracioncalculo/{id}` - Obtener configuración por ID
- `GET /api/configuracioncalculo/sector/{sectorId}` - Configuraciones por sector
- `POST /api/configuracioncalculo` - Crear configuración
- `PUT /api/configuracioncalculo/{id}` - Actualizar configuración
- `DELETE /api/configuracioncalculo/{id}` - Eliminar configuración

## Configuración

### appsettings.json

Configurar antes de ejecutar:
1. `ConnectionStrings:FichadasDB` - Conexión a SQL Server
2. `ConnectionStrings:TangoDB` - Conexión a base de datos de Tango
3. `JwtSettings:SecretKey` - Clave secreta (mínimo 32 caracteres, cambiar en producción)

### Variables de Entorno (Producción)

En producción, usar variables de entorno o Azure Key Vault para:
- Connection strings
- JWT Secret Key
- Credenciales sensibles

## Seguridad

- Nunca commitear `appsettings.Development.json` o `appsettings.Production.json` con credenciales reales
- Las contraseñas se hashean con BCrypt antes de guardar (ver AuthService.cs)
- JWT tokens expiran en 480 minutos (8 horas) - configurable en `appsettings.json`
- Validación de datos en controllers
- Autorización basada en roles usando `[Authorize(Roles = "Admin")]`
- El token JWT se envía en headers como `Authorization: Bearer {token}`

## Dependencias Clave

### Backend (NuGet)
- **Dapper** (2.1.66): Micro-ORM para acceso a datos
- **Microsoft.Data.SqlClient** (6.1.3): Driver de SQL Server
- **BCrypt.Net-Next** (4.0.3): Hashing de contraseñas
- **System.IdentityModel.Tokens.Jwt** (8.2.1): Generación de JWT
- **Microsoft.AspNetCore.Authentication.JwtBearer** (8.0.11): Autenticación JWT
- **EPPlus** (7.5.2): Lectura/escritura de archivos Excel
- **Swashbuckle.AspNetCore** (6.9.0): Generación de Swagger/OpenAPI

### Frontend (npm)
- **React** (19.2.0) y **React DOM** (19.2.0)
- **React Router DOM** (7.9.6): Routing
- **Axios** (1.13.2): Cliente HTTP
- **@tanstack/react-query** (5.90.10): Server state management
- **Zustand** (5.0.8): Client state management
- **React Hook Form** (7.66.0): Gestión de formularios
- **Bootstrap** (5.3.3): UI framework
- **SweetAlert2** (11.26.3): Modales y alertas
- **Vite** (7.2.2): Build tool y dev server

## Próximos Pasos / Mejoras Pendientes

1. Implementar generación de reportes (PDF/Excel) para Planilla Oficial y Adicional
2. Implementar gestión completa de novedades y licencias
3. Implementar sistema de auditoría completo (tabla `ba_auditoria`)
4. Agregar validaciones más robustas en formularios del frontend
5. Implementar paginación en consultas de fichadas
6. Agregar filtros avanzados en pantallas de consulta
7. Implementar dashboard con estadísticas y gráficos
