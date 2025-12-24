using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;
using FichadasAPI.Repositories;

namespace FichadasAPI.Services;

public class FichadaExportService : IFichadaExportService
{
    private readonly DapperContext _context;
    private readonly IFichadaRepository _fichadaRepository;
    private readonly INovedadRepository _novedadRepository;
    private readonly ISectorRepository _sectorRepository;

    public FichadaExportService(
        DapperContext context,
        IFichadaRepository fichadaRepository,
        INovedadRepository novedadRepository,
        ISectorRepository sectorRepository)
    {
        _context = context;
        _fichadaRepository = fichadaRepository;
        _novedadRepository = novedadRepository;
        _sectorRepository = sectorRepository;
    }

    public async Task<ExportarFichadasResult> ExportarFichadasAsync(List<int> idsFichadas)
    {
        var resultado = new ExportarFichadasResult();

        if (idsFichadas == null || !idsFichadas.Any())
        {
            resultado.Errores.Add("No se especificaron fichadas para exportar");
            return resultado;
        }

        try
        {
            // Obtener todas las fichadas a exportar con JOINs a tablas de Tango
            // Incluir información del sector y la novedad de extras configurada
            var fichadasTodasQuery = $@"SELECT f.id_fichadas as IdFichadas, f.empleado_id as EmpleadoId,
                                             f.hora_entrada as HoraEntrada, f.hora_salida as HoraSalida,
                                             f.horas_totales as HorasTotales, f.trabajadas as Trabajadas,
                                             f.extras as Extras, f.adicionales as Adicionales,
                                             f.novedad_id as NovedadId, f.exportada as Exportada,
                                             e.legajo as EmpleadoLegajo, e.sector_id as SectorId,
                                             s.novedad_extras_id as SectorNovedadExtrasId,
                                             n.cod_novedad as NovedadCodigo,
                                             nExtras.cod_novedad as NovedadExtrasCodigo,
                                             tl.ID_LEGAJO as EmpleadoIdTango,
                                             tn.ID_NOVEDAD as NovedadIdTango,
                                             tnExtras.ID_NOVEDAD as NovedadExtrasIdTango
                                      FROM ba_fichadas f
                                      LEFT JOIN ba_empleados e ON f.empleado_id = e.id_empleado
                                      LEFT JOIN ba_sectores s ON e.sector_id = s.id_sector
                                      LEFT JOIN ba_novedades n ON f.novedad_id = n.id_novedad
                                      LEFT JOIN ba_novedades nExtras ON s.novedad_extras_id = nExtras.id_novedad
                                      LEFT JOIN [{_context.TangoDbName}].[dbo].[LEGAJO] tl ON e.legajo = tl.NRO_LEGAJO
                                      LEFT JOIN [{_context.TangoDbName}].[dbo].[NOVEDAD] tn ON n.cod_novedad = tn.COD_NOVEDAD collate Modern_Spanish_CI_AI
                                      LEFT JOIN [{_context.TangoDbName}].[dbo].[NOVEDAD] tnExtras ON nExtras.cod_novedad = tnExtras.COD_NOVEDAD collate Modern_Spanish_CI_AI
                                      WHERE f.id_fichadas IN @Ids";

            List<Fichada> fichadas;
            using (var connection = _context.CreateConnection())
            {
                fichadas = (await connection.QueryAsync<Fichada>(fichadasTodasQuery, new { Ids = idsFichadas })).ToList();
            }

            if (!fichadas.Any())
            {
                resultado.Errores.Add("No se encontraron las fichadas especificadas");
                return resultado;
            }

            // Validar y filtrar fichadas válidas
            var fichadasValidas = new List<Fichada>();
            var fichadasConError = new List<int>();

            foreach (var fichada in fichadas)
            {
                // Validaciones
                if (fichada.Exportada)
                {
                    resultado.Advertencias.Add($"Fichada {fichada.IdFichadas}: Ya fue exportada anteriormente");
                    continue;
                }

                if (!fichada.NovedadId.HasValue)
                {
                    resultado.Errores.Add($"Fichada {fichada.IdFichadas}: No tiene novedad asignada");
                    resultado.FichadasConError++;
                    fichadasConError.Add(fichada.IdFichadas);
                    continue;
                }

                if (!fichada.EmpleadoIdTango.HasValue)
                {
                    resultado.Errores.Add($"Fichada {fichada.IdFichadas}: El empleado con legajo {fichada.EmpleadoLegajo} no existe en Tango");
                    resultado.FichadasConError++;
                    fichadasConError.Add(fichada.IdFichadas);
                    continue;
                }

                if (!fichada.NovedadIdTango.HasValue)
                {
                    resultado.Errores.Add($"Fichada {fichada.IdFichadas}: La novedad '{fichada.NovedadCodigo}' no existe en Tango");
                    resultado.FichadasConError++;
                    fichadasConError.Add(fichada.IdFichadas);
                    continue;
                }

                if (!fichada.HoraEntrada.HasValue)
                {
                    resultado.Errores.Add($"Fichada {fichada.IdFichadas}: No tiene hora de entrada");
                    resultado.FichadasConError++;
                    fichadasConError.Add(fichada.IdFichadas);
                    continue;
                }

                // Validar que si tiene horas extras, el sector tenga configurada la novedad de extras
                if (fichada.Extras.HasValue && fichada.Extras.Value > 0)
                {
                    if (!fichada.SectorNovedadExtrasId.HasValue)
                    {
                        resultado.Errores.Add($"Fichada {fichada.IdFichadas}: Tiene horas extras pero el sector no tiene configurada una novedad para extras");
                        resultado.FichadasConError++;
                        fichadasConError.Add(fichada.IdFichadas);
                        continue;
                    }

                    if (!fichada.NovedadExtrasIdTango.HasValue)
                    {
                        resultado.Errores.Add($"Fichada {fichada.IdFichadas}: La novedad de extras '{fichada.NovedadExtrasCodigo}' no existe en Tango");
                        resultado.FichadasConError++;
                        fichadasConError.Add(fichada.IdFichadas);
                        continue;
                    }
                }

                fichadasValidas.Add(fichada);
            }

            if (!fichadasValidas.Any())
            {
                resultado.Message = "No hay fichadas válidas para exportar";
                return resultado;
            }

            // Crear items de exportación - cada fichada puede generar 1 o 2 items
            // - Trabajadas: se exportan con la novedad de la fichada
            // - Extras: se exportan con la novedad configurada del sector
            // - Adicionales: NO se exportan a Tango
            var itemsExportacion = new List<ExportItem>();

            foreach (var fichada in fichadasValidas)
            {
                // Item para horas trabajadas (si > 0)
                if (fichada.Trabajadas.HasValue && fichada.Trabajadas.Value > 0)
                {
                    itemsExportacion.Add(new ExportItem
                    {
                        IdFichada = fichada.IdFichadas,
                        EmpleadoIdTango = fichada.EmpleadoIdTango.Value,
                        NovedadIdTango = fichada.NovedadIdTango.Value,
                        MinutosAExportar = fichada.Trabajadas.Value,
                        EmpleadoLegajo = fichada.EmpleadoLegajo!.Value,
                        NovedadCodigo = fichada.NovedadCodigo!,
                        TipoHoras = "Trabajadas",
                        FechaFichada = fichada.HoraEntrada!.Value.Date
                    });
                }

                // Item para horas extras (si > 0)
                if (fichada.Extras.HasValue && fichada.Extras.Value > 0)
                {
                    itemsExportacion.Add(new ExportItem
                    {
                        IdFichada = fichada.IdFichadas,
                        EmpleadoIdTango = fichada.EmpleadoIdTango.Value,
                        NovedadIdTango = fichada.NovedadExtrasIdTango!.Value,
                        MinutosAExportar = fichada.Extras.Value,
                        EmpleadoLegajo = fichada.EmpleadoLegajo!.Value,
                        NovedadCodigo = fichada.NovedadExtrasCodigo!,
                        TipoHoras = "Extras",
                        FechaFichada = fichada.HoraEntrada!.Value.Date
                    });
                }
            }

            if (!itemsExportacion.Any())
            {
                resultado.Message = "No hay horas para exportar (trabajadas y extras son 0)";
                return resultado;
            }

            // AGRUPAR items por Legajo (EmpleadoIdTango) + Novedad (NovedadIdTango)
            // NO agrupamos por fecha, todos los items del mismo legajo+novedad van en un solo registro
            var gruposExportacion = itemsExportacion
                .GroupBy(item => new
                {
                    EmpleadoIdTango = item.EmpleadoIdTango,
                    NovedadIdTango = item.NovedadIdTango
                })
                .ToList();

            resultado.Advertencias.Add($"Se procesaron {fichadasValidas.Count} fichadas en {itemsExportacion.Count} items (trabajadas/extras por separado) agrupados en {gruposExportacion.Count} registros para Tango");

            // Procesar cada grupo
            foreach (var grupo in gruposExportacion)
            {
                try
                {
                    var itemsDelGrupo = grupo.ToList();
                    var primerItem = itemsDelGrupo.First();

                    // Sumar todos los minutos del grupo y convertir a horas decimales
                    decimal totalHorasGrupo = itemsDelGrupo.Sum(item => (decimal)item.MinutosAExportar / 60m);

                    // Insertar UN SOLO registro en NOVEDAD_REGISTRADA de Tango por este grupo
                    // FECHA_NOVEDAD se establece como la fecha actual (HOY)
                    var insertQuery = $@"INSERT INTO [{_context.TangoDbName}].[dbo].[NOVEDAD_REGISTRADA]
                                      (ID_LEGAJO, ID_NOVEDAD, FECHA_NOVEDAD, CANT_NOVEDAD, ORIGEN_CLOUD, ORIGEN_NOVEDAD)
                                      VALUES (@IdLegajo, @IdNovedad, @Fecha, @Cantidad, @OrigenCloud, @OrigenNovedad)";

                    using (var connection = _context.CreateConnection())
                    {
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            IdLegajo = grupo.Key.EmpleadoIdTango,
                            IdNovedad = grupo.Key.NovedadIdTango,
                            Fecha = DateTime.Now.Date, // Fecha actual, NO la fecha de las fichadas
                            Cantidad = totalHorasGrupo,
                            OrigenCloud = "Externo",
                            OrigenNovedad = "Externo"
                        });
                    }

                    // Agregar mensaje informativo del grupo
                    var fechasRango = itemsDelGrupo.Select(item => item.FechaFichada)
                                                   .Distinct()
                                                   .OrderBy(f => f)
                                                   .ToList();
                    var fechaMin = fechasRango.First().ToString("dd/MM/yyyy");
                    var fechaMax = fechasRango.Last().ToString("dd/MM/yyyy");
                    var rangoFechas = fechaMin == fechaMax ? fechaMin : $"{fechaMin} a {fechaMax}";

                    var tiposHoras = string.Join(", ", itemsDelGrupo.Select(i => i.TipoHoras).Distinct());

                    resultado.Advertencias.Add(
                        $"Legajo {primerItem.EmpleadoLegajo} - Novedad {primerItem.NovedadCodigo} ({tiposHoras}): " +
                        $"{itemsDelGrupo.Count} items ({rangoFechas}) = {totalHorasGrupo:F2} horas totales. " +
                        $"Fecha exportación: {DateTime.Now:dd/MM/yyyy}"
                    );
                }
                catch (Exception ex)
                {
                    var itemsDelGrupo = grupo.ToList();
                    foreach (var item in itemsDelGrupo)
                    {
                        resultado.Errores.Add($"Fichada {item.IdFichada}: Error al exportar {item.TipoHoras} - {ex.Message}");
                        resultado.FichadasConError++;
                    }
                }
            }

            // Marcar TODAS las fichadas válidas como exportadas (una sola vez)
            var idsFichadasExportadas = fichadasValidas.Select(f => f.IdFichadas).ToList();
            var updateQuery = @"UPDATE ba_fichadas
                              SET exportada = 1, fecha_exportacion = GETDATE()
                              WHERE id_fichadas IN @Ids";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(updateQuery, new { Ids = idsFichadasExportadas });
            }

            resultado.FichadasExportadas = fichadasValidas.Count;

            // Generar mensaje de resultado
            if (resultado.FichadasExportadas > 0)
            {
                resultado.Message = $"Se exportaron {resultado.FichadasExportadas} fichadas ({itemsExportacion.Count} items trabajadas/extras) agrupados en {gruposExportacion.Count} registros a Tango";
                if (resultado.FichadasConError > 0)
                {
                    resultado.Message += $". {resultado.FichadasConError} items con errores";
                }
            }
            else
            {
                resultado.Message = "No se pudieron exportar fichadas";
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Errores.Add($"Error general al exportar fichadas: {ex.Message}");
            resultado.Message = "Error al exportar fichadas";
            return resultado;
        }
    }

    // Clase helper para representar items de exportación
    private class ExportItem
    {
        public int IdFichada { get; set; }
        public int EmpleadoIdTango { get; set; }
        public int NovedadIdTango { get; set; }
        public int MinutosAExportar { get; set; }
        public int EmpleadoLegajo { get; set; }
        public string NovedadCodigo { get; set; } = string.Empty;
        public string TipoHoras { get; set; } = string.Empty; // "Trabajadas" o "Extras"
        public DateTime FechaFichada { get; set; }
    }
}
