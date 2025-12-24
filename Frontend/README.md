# Frontend - Sistema de Fichadas Colombraro

Frontend desarrollado con React, TypeScript y SWC (Vite).

## Tecnologías

- **React 19** - Biblioteca de UI
- **TypeScript** - Tipado estático
- **Vite + SWC** - Build tool ultra rápido
- **React Router DOM** - Enrutamiento
- **Axios** - Cliente HTTP
- **Zustand** - Estado global
- **React Query** - Gestión de estado del servidor
- **React Hook Form** - Formularios

## Instalación

```bash
# Instalar dependencias
npm install

# Ejecutar en desarrollo
npm run dev

# Compilar para producción
npm run build

# Previsualizar build de producción
npm run preview
```

## Configuración

El frontend está configurado para conectarse al backend en `http://localhost:5210`. Si el backend está en otra URL, modifica `src/services/api.ts`:

```typescript
const api = axios.create({
  baseURL: 'http://localhost:5210/api',  // Cambiar aquí
  ...
});
```

## Características

### Autenticación
- Login con JWT
- Protección de rutas
- Persistencia de sesión en localStorage
- Logout automático en token expirado

### Páginas Implementadas
- **Login**: Autenticación de usuarios
- **Home**: Dashboard principal
- **Empleados**: Listado y filtrado por sector
- **Fichadas**: Consulta de fichadas por rango de fechas
- **Sectores**: Gestión de sectores (solo Admin)
- **Horarios**: Gestión de horarios de turno (solo Admin)

## Scripts Disponibles

- `npm run dev` - Inicia el servidor de desarrollo en http://localhost:5173
- `npm run build` - Compila el proyecto para producción
- `npm run preview` - Previsualiza el build de producción

## Próximas Funcionalidades

- Importación de fichadas desde Excel
- Generación de reportes
- Gestión de novedades y licencias
- Dashboard con estadísticas
