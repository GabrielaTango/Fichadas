using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class FichadaRepository : IFichadaRepository
{
    private readonly DapperContext _context;

    public FichadaRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Fichada>> GetAllAsync()
    {
        var query = @"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                             f.horario_turno_id as HorarioTurnoId, f.horas_totales as HorasTotales,
                             f.trabajadas as Trabajadas, f.extras as Extras,
                             f.adicionales as Adicionales, f.codigo_novedad as CodigoNovedad,
                             f.novedad_id as NovedadId,
                             f.exportada as Exportada, f.fecha_exportacion as FechaExportacion,
                             n.desc_novedad as NovedadDescripcion, n.cod_novedad as NovedadCodigo,
                             e.legajo as EmpleadoLegajo
                      FROM ba_fichadas f
                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Fichada>(query);
    }

    public async Task<Fichada?> GetByIdAsync(int id)
    {
        var query = @"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                             f.horario_turno_id as HorarioTurnoId, f.horas_totales as HorasTotales,
                             f.trabajadas as Trabajadas, f.extras as Extras,
                             f.adicionales as Adicionales, f.codigo_novedad as CodigoNovedad,
                             f.novedad_id as NovedadId,
                             f.exportada as Exportada, f.fecha_exportacion as FechaExportacion,
                             n.desc_novedad as NovedadDescripcion, n.cod_novedad as NovedadCodigo,
                             e.legajo as EmpleadoLegajo
                      FROM ba_fichadas f
                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
                      WHERE f.id_fichadas = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Fichada>(query, new { Id = id });
    }

    public async Task<IEnumerable<Fichada>> GetByEmpleadoAsync(int empleadoId)
    {
        var query = @"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                             f.horario_turno_id as HorarioTurnoId, f.horas_totales as HorasTotales,
                             f.trabajadas as Trabajadas, f.extras as Extras,
                             f.adicionales as Adicionales, f.codigo_novedad as CodigoNovedad,
                             f.novedad_id as NovedadId,
                             f.exportada as Exportada, f.fecha_exportacion as FechaExportacion,
                             n.desc_novedad as NovedadDescripcion, n.cod_novedad as NovedadCodigo,
                             e.legajo as EmpleadoLegajo
                      FROM ba_fichadas f
                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
                      WHERE f.empleado_id = @EmpleadoId
                      ORDER BY f.hora_entrada DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Fichada>(query, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Fichada>> GetByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        fechaDesde = fechaDesde.Date;
        fechaHasta = fechaHasta.Date.AddDays(1).AddMicroseconds(-1);

        var query = @"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                             f.horario_turno_id as HorarioTurnoId, f.horas_totales as HorasTotales,
                             f.trabajadas as Trabajadas, f.extras as Extras,
                             f.adicionales as Adicionales, f.codigo_novedad as CodigoNovedad,
                             f.novedad_id as NovedadId,
                             f.exportada as Exportada, f.fecha_exportacion as FechaExportacion,
                             n.desc_novedad as NovedadDescripcion, n.cod_novedad as NovedadCodigo,
                             e.legajo as EmpleadoLegajo
                      FROM ba_fichadas f
                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
                      WHERE f.hora_entrada >= @FechaDesde AND f.hora_entrada <= @FechaHasta
                      ORDER BY f.hora_entrada";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Fichada>(query, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }

    public async Task<int> CreateAsync(Fichada fichada)
    {
        var query = @"INSERT INTO ba_fichadas (empleado_id, hora_entrada, hora_salida, horario_turno_id,
                                               horas_totales, trabajadas, extras, adicionales, codigo_novedad, novedad_id)
                      VALUES (@EmpleadoId, @HoraEntrada, @HoraSalida, @HorarioTurnoId,
                              @HorasTotales, @Trabajadas, @Extras, @Adicionales, @CodigoNovedad, @NovedadId);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, fichada);
    }

    public async Task<bool> UpdateAsync(Fichada fichada)
    {
        var query = @"UPDATE ba_fichadas
                      SET empleado_id = @EmpleadoId, hora_entrada = @HoraEntrada,
                          hora_salida = @HoraSalida, horario_turno_id = @HorarioTurnoId,
                          horas_totales = @HorasTotales, trabajadas = @Trabajadas,
                          extras = @Extras, adicionales = @Adicionales, codigo_novedad = @CodigoNovedad,
                          novedad_id = @NovedadId
                      WHERE id_fichadas = @IdFichadas";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, fichada);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_fichadas WHERE id_fichadas = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Fichada>> GetByFiltrosAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? busquedaEmpleado, bool? exportada)
    {
        var query = @"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                             f.horario_turno_id as HorarioTurnoId, f.horas_totales as HorasTotales,
                             f.trabajadas as Trabajadas, f.extras as Extras,
                             f.adicionales as Adicionales, f.codigo_novedad as CodigoNovedad,
                             f.novedad_id as NovedadId,
                             f.exportada as Exportada, f.fecha_exportacion as FechaExportacion,
                             n.desc_novedad as NovedadDescripcion, n.cod_novedad as NovedadCodigo,
                             e.legajo as EmpleadoLegajo,
                             e.nombre as EmpleadoNombre,
                             s.nombre as SectorNombre
                      FROM ba_fichadas f
                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector
                      WHERE 1=1";

        var parameters = new DynamicParameters();

        // Filtro por fecha
        if (fechaDesde.HasValue && fechaHasta.HasValue)
        {
            var desde = fechaDesde.Value.Date;
            var hasta = fechaHasta.Value.Date.AddDays(1).AddMicroseconds(-1);
            query += " AND f.hora_entrada >= @FechaDesde AND f.hora_entrada <= @FechaHasta";
            parameters.Add("FechaDesde", desde);
            parameters.Add("FechaHasta", hasta);
        }

        // Filtro por empleado (nombre o legajo)
        if (!string.IsNullOrWhiteSpace(busquedaEmpleado))
        {
            query += " AND (e.nombre LIKE @BusquedaEmpleado OR CAST(e.legajo AS VARCHAR) LIKE @BusquedaEmpleado)";
            parameters.Add("BusquedaEmpleado", $"%{busquedaEmpleado}%");
        }

        // Filtro por exportada
        if (exportada.HasValue)
        {
            query += " AND f.exportada = @Exportada";
            parameters.Add("Exportada", exportada.Value);
        }

        query += " ORDER BY f.hora_entrada DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Fichada>(query, parameters);
    }

    public async Task<int> ImportarFichadasAsync(List<FichadaExcel> fichadas)
    {
        // NOTA: Este método está obsoleto.
        // La importación ahora se maneja en FichadaImportService
        return await Task.FromResult(0);
    }
}
