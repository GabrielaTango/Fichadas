using FichadasAPI.Models;
using FichadasAPI.Repositories;
using OfficeOpenXml;
using System.Globalization;

namespace FichadasAPI.Services;

public class FichadaImportService : IFichadaImportService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IFichadaRepository _fichadaRepository;
    private readonly IHorasCalculoService _horasCalculoService;
    private readonly ISectorRepository _sectorRepository;

    public FichadaImportService(
        IEmpleadoRepository empleadoRepository,
        IFichadaRepository fichadaRepository,
        IHorasCalculoService horasCalculoService,
        ISectorRepository sectorRepository)
    {
        _empleadoRepository = empleadoRepository;
        _fichadaRepository = fichadaRepository;
        _horasCalculoService = horasCalculoService;
        _sectorRepository = sectorRepository;
    }

    public async Task<FichadaImportResult> ImportarDesdeExcelAsync(Stream excelStream)
    {
        var result = new FichadaImportResult();

        // Configurar EPPlus para uso no comercial
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0]; // Primera hoja

            if (worksheet == null)
            {
                result.Errores.Add("El archivo Excel no contiene hojas de trabajo");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // Empezar desde la fila 2 (asumiendo que la fila 1 es encabezado)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Columna A: idPersona (legajo)
                    var legajoCell = worksheet.Cells[row, 1].Value;
                    if (legajoCell == null)
                    {
                        result.FichadasIgnoradas++;
                        continue; // Fila vacía, ignorar
                    }

                    int legajo = Convert.ToInt32(legajoCell);

                    // Columna G: fecha (yyyyMMdd)
                    var fechaCell = worksheet.Cells[row, 7].Value;
                    if (fechaCell == null)
                    {
                        result.Errores.Add($"Fila {row}: Fecha vacía para legajo {legajo}");
                        result.FichadasIgnoradas++;
                        continue;
                    }

                    // Parsear la fecha
                    DateTime fecha;
                    if (fechaCell is DateTime fechaDateTime)
                    {
                        fecha = fechaDateTime;
                    }
                    else
                    {
                        string fechaStr = fechaCell.ToString()!;
                        if (!DateTime.TryParseExact(fechaStr, "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out fecha))
                        {
                            result.Errores.Add($"Fila {row}: Formato de fecha inválido '{fechaStr}' para legajo {legajo}");
                            result.FichadasIgnoradas++;
                            continue;
                        }
                    }

                    // Columna H: fichadas (horas separadas por ;)
                    var fichadasCell = worksheet.Cells[row, 8].Value;
                    if (fichadasCell == null || string.IsNullOrWhiteSpace(fichadasCell.ToString()))
                    {
                        result.Errores.Add($"Fila {row}: Fichadas vacías para legajo {legajo}");
                        result.FichadasIgnoradas++;
                        continue;
                    }

                    string fichadasStr = fichadasCell.ToString()!;

                    // Verificar que el empleado existe
                    var empleado = await _empleadoRepository.GetByLegajoAsync(legajo);
                    if (empleado == null)
                    {
                        result.Errores.Add($"Fila {row}: Empleado con legajo {legajo} no encontrado");
                        result.FichadasIgnoradas++;
                        continue;
                    }

                    // Parsear las horas
                    var (horaEntrada, horaSalida, error) = ParsearFichadas(fichadasStr, fecha);

                    if (error != null)
                    {
                        result.Errores.Add($"Fila {row}: {error}");
                        result.FichadasIgnoradas++;
                        continue;
                    }

                    // Calcular las horas trabajadas
                    var calculo = await _horasCalculoService.CalcularHorasAsync(
                        empleado.IdEmpleado,
                        horaEntrada.Value,
                        horaSalida.Value);

                    // Obtener el sector para usar la novedad por defecto si está configurada
                    int? novedadId = null;
                    if (empleado.SectorId.HasValue)
                    {
                        var sector = await _sectorRepository.GetByIdAsync(empleado.SectorId.Value);
                        if (sector?.NovedadTrabajadasId.HasValue == true)
                        {
                            novedadId = sector.NovedadTrabajadasId.Value;
                        }
                    }

                    // Crear la fichada con los datos calculados
                    var fichada = new Fichada
                    {
                        EmpleadoId = empleado.IdEmpleado,
                        HoraEntrada = horaEntrada,
                        HoraSalida = horaSalida,
                        HorasTotales = calculo.HorasTotales,
                        Trabajadas = calculo.HorasTrabajadas,
                        Extras = calculo.HorasExtras,
                        Adicionales = calculo.HorasAdicionales,
                        NovedadId = novedadId
                    };

                    await _fichadaRepository.CreateAsync(fichada);
                    result.FichadasImportadas++;

                    // Si hay advertencias del cálculo, agregarlas al resultado
                    if (calculo.Advertencias.Any())
                    {
                        foreach (var advertencia in calculo.Advertencias)
                        {
                            result.Errores.Add($"Fila {row} - Legajo {legajo}: {advertencia}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Fila {row}: Error inesperado - {ex.Message}");
                    result.FichadasIgnoradas++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Errores.Add($"Error al procesar el archivo: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Parsea la cadena de fichadas con el formato especial:
    /// - Horas separadas por ';'
    /// - Si la segunda hora tiene '+' en vez de ':', significa que salió al día siguiente
    /// Ejemplos:
    /// - "08:00;17:00" -> entrada 08:00, salida 17:00 mismo día
    /// - "08:00;17+30" -> entrada 08:00, salida 17:30 del día siguiente
    /// </summary>
    private (DateTime? entrada, DateTime? salida, string? error) ParsearFichadas(string fichadasStr, DateTime fecha)
    {
        try
        {
            var horas = fichadasStr.Split(';');

            if (horas.Length < 2)
            {
                return (null, null, "Debe haber al menos 2 horas (entrada y salida)");
            }

            // Primera hora: entrada
            if (!TryParseHora(horas[0], fecha, out var horaEntrada))
            {
                return (null, null, $"Formato de hora de entrada inválido: '{horas[0]}'");
            }

            // Segunda hora: salida (puede tener + indicando día siguiente)
            string horaSalidaStr = horas[1];
            bool esDiaSiguiente = horaSalidaStr.Contains('+');

            if (esDiaSiguiente)
            {
                // Reemplazar + por : para parsear
                horaSalidaStr = horaSalidaStr.Replace('+', ':');
            }

            if (!TryParseHora(horaSalidaStr, fecha, out var horaSalida))
            {
                return (null, null, $"Formato de hora de salida inválido: '{horas[1]}'");
            }

            // Si tiene +, sumar un día a la salida
            if (esDiaSiguiente)
            {
                horaSalida = horaSalida.AddDays(1);
            }

            return (horaEntrada, horaSalida, null);
        }
        catch (Exception ex)
        {
            return (null, null, $"Error al parsear fichadas: {ex.Message}");
        }
    }

    /// <summary>
    /// Intenta parsear una hora en formato HH:mm y combinarla con la fecha
    /// </summary>
    private bool TryParseHora(string horaStr, DateTime fecha, out DateTime resultado)
    {
        resultado = DateTime.MinValue;

        if (string.IsNullOrWhiteSpace(horaStr))
            return false;

        // Formato esperado: HH:mm o H:mm
        if (!TimeSpan.TryParseExact(horaStr.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out var timeSpan))
        {
            return false;
        }

        resultado = fecha.Date.Add(timeSpan);
        return true;
    }
}
