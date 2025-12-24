# Sistema de Fichadas Colombraro

Sistema de administración de control de personal (Fichadas y Licencias) desarrollado con ASP.NET Core 8 Web API y React con TypeScript.

## Tecnologías

- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: React con TypeScript (Pendiente)
- **Base de Datos**: SQL Server
- **ORM**: Dapper
- **Autenticación**: JWT Bearer

## Estructura del Proyecto

```
Colombraro Fichadas/
├── Database/
│   ├── creacion.sql                    # Script MySQL original
│   └── CreacionSQLServer.sql           # Script convertido a SQL Server
├── Backend/
│   ├── FichadasSystem.sln              # Solución de Visual Studio
│   └── FichadasAPI/
│       ├── Controllers/                # Endpoints de la API
│       ├── Models/                     # DTOs y entidades
│       ├── Repositories/               # Capa de acceso a datos
│       ├── Services/                   # Lógica de negocio
│       ├── Data/                       # Contexto de Dapper
│       ├── Program.cs                  # Configuración de la aplicación
│       └── appsettings.json            # Configuración
├── Frontend/                           # Por crear
├── CLAUDE.md                           # Documentación para Claude Code
└── README.md                           # Este archivo
```

## Configuración Inicial

### 1. Base de Datos

1. Abre SQL Server Management Studio
2. Ejecuta el script `Database/CreacionSQLServer.sql`
3. Actualiza el connection string en `Backend/FichadasAPI/appsettings.json`:

```json
"ConnectionStrings": {
  "FichadasDB": "Server=TU_SERVIDOR;Database=FichadasDB;Trusted_Connection=True;TrustServerCertificate=True;",
  "TangoDB": "Server=TU_SERVIDOR;Database=TangoDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Backend

El proyecto ya está creado y listo para usar. Para ejecutarlo:

```bash
# Desde la raíz del proyecto
cd Backend/FichadasAPI
dotnet run

# O compilar toda la solución
cd Backend
dotnet build
```

La API estará disponible en:
- HTTP: `http://localhost:5210`
- HTTPS: `https://localhost:7210` (si está configurado)
- Swagger UI: `http://localhost:5210/swagger`

## Endpoints de la API

### Autenticación (Sin autenticación requerida)
- `POST /api/auth/login` - Login de usuario
- `POST /api/auth/change-password` - Cambiar contraseña

### Sectores (Requiere autenticación)
- `GET /api/sectores` - Obtener todos los sectores
- `GET /api/sectores/{id}` - Obtener sector por ID
- `POST /api/sectores` - Crear sector (Solo Admin)
- `PUT /api/sectores/{id}` - Actualizar sector (Solo Admin)
- `DELETE /api/sectores/{id}` - Eliminar sector (Solo Admin)

### Empleados (Requiere autenticación)
- `GET /api/empleados` - Obtener todos los empleados
- `GET /api/empleados/{id}` - Obtener empleado por ID
- `GET /api/empleados/legajo/{legajo}` - Obtener empleado por legajo
- `GET /api/empleados/sector/{sectorId}` - Obtener empleados por sector
- `POST /api/empleados` - Crear empleado (Solo Admin)
- `PUT /api/empleados/{id}` - Actualizar empleado (Solo Admin)
- `DELETE /api/empleados/{id}` - Eliminar empleado (Solo Admin)

### Fichadas (Requiere autenticación)
- `GET /api/fichadas` - Obtener todas las fichadas
- `GET /api/fichadas/{id}` - Obtener fichada por ID
- `GET /api/fichadas/empleado/{empleadoId}` - Obtener fichadas por empleado
- `GET /api/fichadas/rango?fechaDesde=&fechaHasta=` - Obtener fichadas por rango
- `POST /api/fichadas` - Crear fichada
- `POST /api/fichadas/importar` - Importar fichadas desde Excel
- `PUT /api/fichadas/{id}` - Actualizar fichada
- `DELETE /api/fichadas/{id}` - Eliminar fichada (Solo Admin)

### Horarios de Turno (Requiere autenticación)
- `GET /api/horariosturno` - Obtener todos los horarios de turno
- `GET /api/horariosturno/{id}` - Obtener horario por ID
- `GET /api/horariosturno/sector/{sectorId}` - Obtener horarios por sector
- `GET /api/horariosturno/verano/{esVerano}` - Obtener horarios por temporada (true/false)
- `POST /api/horariosturno` - Crear horario de turno (Solo Admin)
- `PUT /api/horariosturno/{id}` - Actualizar horario de turno (Solo Admin)
- `DELETE /api/horariosturno/{id}` - Eliminar horario de turno (Solo Admin)

## Seguridad

- Todos los endpoints (excepto `/api/auth/login`) requieren autenticación JWT
- Las contraseñas se hashean con BCrypt
- Los tokens JWT expiran en 8 horas (configurable)
- Roles: `Admin` y `User`

## Configuración Importante

### Secret Key para JWT

**IMPORTANTE**: Antes de usar en producción, cambia la clave secreta en `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "TU_CLAVE_SECRETA_DE_MINIMO_32_CARACTERES"
}
```

### CORS

El backend está configurado para permitir peticiones desde:
- `http://localhost:3000` (Create React App)
- `http://localhost:5173` (Vite)

Modifica esto en `Program.cs` según tus necesidades.

## Tareas Pendientes

### Backend
- [ ] Implementar lógica completa de importación de fichadas desde Excel
- [ ] Implementar cálculo de horas normales, extras y adicionales según reglas del negocio
- [ ] Crear servicios de generación de reportes (PDF/Excel)
- [ ] Implementar gestión completa de novedades y licencias
- [ ] Agregar validaciones de datos más robustas
- [ ] Implementar sistema de auditoría completo

### Frontend
- [ ] Crear proyecto React con TypeScript
- [ ] Implementar sistema de login
- [ ] Crear dashboard principal
- [ ] Pantalla de gestión de empleados
- [ ] Pantalla de importación de fichadas
- [ ] Pantalla de gestión de novedades y licencias
- [ ] Generación de reportes

## Reglas de Negocio

El sistema calcula horas según estas reglas (ver `Proyecto.txt` para detalles completos):

### Empleados de Máquinas
- Turno mañana (06:00-18:00): 9h normales + 1h extra oficial + 2h extras adicionales
- Turno tarde (18:00-06:00): 9h normales + 1h extra oficial + 2h extras adicionales

### Empleados de Expedición
- Verano (07:00-18:00): 9h normales + 1h extra oficial + 1h extra adicional
- Invierno (08:00-18:00): 9h normales + 1h extra oficial (sin adicionales)

### Empleados de Administración
- Horario 08:30-18:00 o 09:00-18:00
- 10 horas (1 hora es almuerzo, no cuenta como extra)

### Tolerancia de Llegadas Tarde
- Tolerancia: 5 minutos
- 6-30 minutos tarde: descuento de 30 minutos
- 31+ minutos tarde: descuento de 1 hora

## Comandos Útiles

```bash
# Todos los comandos se ejecutan desde la carpeta Backend/FichadasAPI

# Compilar el proyecto
cd Backend/FichadasAPI
dotnet build

# Ejecutar el proyecto
dotnet run

# Ejecutar con hot reload
dotnet watch run

# Restaurar paquetes
dotnet restore

# Limpiar build
dotnet clean

# Compilar toda la solución (desde Backend/)
cd Backend
dotnet build
```

## Desarrollo

Para más detalles sobre la arquitectura y desarrollo, consulta el archivo `CLAUDE.md`.

## Licencia

Proyecto privado para Colombraro.
