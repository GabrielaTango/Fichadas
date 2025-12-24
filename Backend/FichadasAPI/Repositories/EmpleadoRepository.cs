using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly DapperContext _context;

    public EmpleadoRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        var query = @"SELECT e.id_empleado as IdEmpleado, e.nombre as Nombre, e.legajo as Legajo,
                             e.sector_id as SectorId, s.nombre as SectorNombre,
                             e.fecha_inicio_rotacion as FechaInicioRotacion
                      FROM ba_empleados e
                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(query);
    }

    public async Task<Empleado?> GetByIdAsync(int id)
    {
        var query = @"SELECT e.id_empleado as IdEmpleado, e.nombre as Nombre, e.legajo as Legajo,
                             e.sector_id as SectorId, s.nombre as SectorNombre,
                             e.fecha_inicio_rotacion as FechaInicioRotacion
                      FROM ba_empleados e
                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector
                      WHERE e.id_empleado = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Empleado>(query, new { Id = id });
    }

    public async Task<Empleado?> GetByLegajoAsync(int legajo)
    {
        var query = @"SELECT e.id_empleado as IdEmpleado, e.nombre as Nombre, e.legajo as Legajo,
                             e.sector_id as SectorId, s.nombre as SectorNombre,
                             e.fecha_inicio_rotacion as FechaInicioRotacion
                      FROM ba_empleados e
                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector
                      WHERE e.legajo = @Legajo";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Empleado>(query, new { Legajo = legajo });
    }

    public async Task<IEnumerable<Empleado>> GetBySectorAsync(int sectorId)
    {
        var query = @"SELECT e.id_empleado as IdEmpleado, e.nombre as Nombre, e.legajo as Legajo,
                             e.sector_id as SectorId, s.nombre as SectorNombre,
                             e.fecha_inicio_rotacion as FechaInicioRotacion
                      FROM ba_empleados e
                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector
                      WHERE e.sector_id = @SectorId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(query, new { SectorId = sectorId });
    }

    public async Task<int> CreateAsync(Empleado empleado)
    {
        var query = @"INSERT INTO ba_empleados (nombre, legajo, sector_id, fecha_inicio_rotacion)
                      VALUES (@Nombre, @Legajo, @SectorId, @FechaInicioRotacion);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, empleado);
    }

    public async Task<bool> UpdateAsync(Empleado empleado)
    {
        var query = @"UPDATE ba_empleados
                      SET nombre = @Nombre, legajo = @Legajo, sector_id = @SectorId,
                          fecha_inicio_rotacion = @FechaInicioRotacion
                      WHERE id_empleado = @IdEmpleado";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, empleado);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_empleados WHERE id_empleado = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<ImportarEmpleadosResult> ImportarDesdeTangoAsync()
    {
        var resultado = new ImportarEmpleadosResult();

        try
        {
            // Leer empleados desde Tango
            var queryTango = $@"SELECT NRO_LEGAJO as NroLegajo,
                                      APELLIDO + ', ' + NOMBRE AS Nombre
                               FROM [{_context.TangoDbName}].[dbo].[LEGAJO]";

            List<EmpleadoTango> empleadosTango;
            using (var tangoConn = _context.CreateConnection())
            {
                empleadosTango = (await tangoConn.QueryAsync<EmpleadoTango>(queryTango)).ToList();
            }

            if (!empleadosTango.Any())
            {
                resultado.Errores.Add("No se encontraron empleados en la base de datos de Tango");
                return resultado;
            }

            // Obtener empleados existentes en FichadasDB
            var empleadosExistentes = (await GetAllAsync()).ToList();

            using var connection = _context.CreateConnection();

            foreach (var empleadoTango in empleadosTango)
            {
                try
                {
                    // Verificar si el empleado ya existe
                    var empleadoExistente = empleadosExistentes.FirstOrDefault(e => e.Legajo == empleadoTango.NroLegajo);

                    if (empleadoExistente != null)
                    {
                        // Actualizar el nombre si cambió
                        if (empleadoExistente.Nombre != empleadoTango.Nombre)
                        {
                            var updateQuery = @"UPDATE ba_empleados
                                              SET nombre = @Nombre
                                              WHERE legajo = @Legajo";
                            await connection.ExecuteAsync(updateQuery, new
                            {
                                Nombre = empleadoTango.Nombre,
                                Legajo = empleadoTango.NroLegajo
                            });
                            resultado.EmpleadosActualizados++;
                        }
                        else
                        {
                            resultado.EmpleadosExistentes++;
                        }
                    }
                    else
                    {
                        // Insertar nuevo empleado sin sector
                        var insertQuery = @"INSERT INTO ba_empleados (nombre, legajo, sector_id)
                                          VALUES (@Nombre, @Legajo, NULL)";
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            Nombre = empleadoTango.Nombre,
                            Legajo = empleadoTango.NroLegajo
                        });
                        resultado.EmpleadosImportados++;
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Error al importar legajo {empleadoTango.NroLegajo}: {ex.Message}");
                }
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Errores.Add($"Error general al importar empleados: {ex.Message}");
            return resultado;
        }
    }
}
