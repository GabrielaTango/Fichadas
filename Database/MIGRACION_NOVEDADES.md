# Migración de Conceptos a Novedades

Este documento explica cómo migrar el sistema de "conceptos" a "novedades" en la base de datos.

## 📋 Escenarios

### Escenario 1: Base de datos EXISTENTE con datos (MIGRACIÓN)

Si ya tienes una base de datos con la tabla `ba_conceptos` y datos:

**Ejecutar en orden:**
1. **Hacer BACKUP de la base de datos** (¡IMPORTANTE!)
2. Ejecutar `MigrarConceptosANovedades.sql`

Este script hace:
- ✅ Crea la tabla `ba_novedades`
- ✅ Migra todos los datos de `ba_conceptos` a `ba_novedades`
- ✅ Agrega la columna `novedad_id` a `ba_fichadas`
- ✅ Migra las referencias de `concepto_id` a `novedad_id` en todas las fichadas
- ✅ Elimina la columna `concepto_id` de `ba_fichadas`
- ✅ Renombra `ba_conceptos` a `ba_conceptos_OLD` como backup
- ✅ Crea todos los índices y foreign keys necesarios

**Después de ejecutar:**
- Verificar que todo funciona correctamente
- Probar la aplicación completamente
- Si todo está OK, eliminar `ba_conceptos_OLD`:
  ```sql
  DROP TABLE ba_conceptos_OLD;
  ```

---

### Escenario 2: Instalación NUEVA o base de datos LIMPIA

Si estás instalando el sistema por primera vez o no tienes datos:

**Ejecutar en orden:**
1. `CrearTablaNovedades.sql`
2. `AgregarNovedadAFichadas.sql`

---

## 🔍 Estructura de la tabla ba_novedades

```sql
CREATE TABLE ba_novedades (
    id_novedad INT IDENTITY(1,1) PRIMARY KEY,
    cod_novedad NVARCHAR(50) NOT NULL UNIQUE,    -- Código de novedad (ej: "LIC", "AUS")
    desc_novedad NVARCHAR(200) NOT NULL,         -- Descripción
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_modificacion DATETIME DEFAULT GETDATE()
);
```

**Diferencias con ba_conceptos:**
- ❌ Ya NO usa `id_concepto_tango` (innecesario)
- ❌ Ya NO usa `nro_concepto` (número de concepto)
- ✅ Usa `cod_novedad` (código de texto, más flexible)
- ✅ Mapea directamente con `COD_NOVEDAD` de Tango

---

## 📊 Consulta SQL de Tango

El sistema importa novedades desde Tango usando:

```sql
SELECT ID_NOVEDAD, COD_NOVEDAD, DESC_NOVEDAD
FROM NOVEDAD
```

---

## ⚠️ IMPORTANTE: Migración de datos

Durante la migración, el script convierte:
- `nro_concepto` → `cod_novedad` (como texto)
- `desc_concepto` → `desc_novedad`

Por ejemplo:
- `nro_concepto: 1001` se convierte en `cod_novedad: "1001"`
- `desc_concepto: "Licencia"` se convierte en `desc_novedad: "Licencia"`

**RECOMENDACIÓN:**
Después de migrar, ejecuta la importación desde Tango para actualizar con los códigos reales:
1. Ir a la aplicación web
2. Navegar a "Novedades" → pestaña "Disponibles en Tango"
3. Hacer clic en "Importar Todas"

Esto reemplazará los números con los códigos de texto reales de Tango (ej: "LIC", "AUS", "VAC", etc.)

---

## 🧪 Verificación post-migración

Ejecutar estas consultas para verificar:

```sql
-- Ver todas las novedades
SELECT * FROM ba_novedades;

-- Ver fichadas con novedades asignadas
SELECT
    f.id_fichadas,
    e.nombre AS empleado,
    f.hora_entrada,
    n.cod_novedad,
    n.desc_novedad
FROM ba_fichadas f
LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
WHERE f.novedad_id IS NOT NULL;

-- Verificar que la tabla antigua existe como backup
SELECT * FROM ba_conceptos_OLD;
```

---

## 🔧 Resolución de problemas

### Error: "Cannot drop the table 'ba_conceptos', because it does not exist or you do not have permission"
✅ Normal si es una instalación nueva. Continuar con la ejecución.

### Error: "Violation of UNIQUE KEY constraint"
⚠️ Ya existen novedades duplicadas. Revisar manualmente los datos.

### Error: "The UPDATE statement conflicted with the FOREIGN KEY constraint"
⚠️ Hay referencias inconsistentes. Contactar soporte.

---

## 📞 Soporte

Si tienes problemas durante la migración:
1. **NO CONTINUAR** si hay errores
2. Revisar el mensaje de error
3. Restaurar el backup si es necesario
4. Consultar con el equipo de desarrollo
