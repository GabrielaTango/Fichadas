using FichadasAPI.Models;
using FichadasAPI.Repositories;
using FichadasAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FichadasController : ControllerBase
{
    private readonly IFichadaRepository _fichadaRepository;
    private readonly IFichadaImportService _fichadaImportService;
    private readonly IHorasCalculoService _horasCalculoService;
    private readonly IFichadaExportService _fichadaExportService;

    public FichadasController(
        IFichadaRepository fichadaRepository,
        IFichadaImportService fichadaImportService,
        IHorasCalculoService horasCalculoService,
        IFichadaExportService fichadaExportService)
    {
        _fichadaRepository = fichadaRepository;
        _fichadaImportService = fichadaImportService;
        _horasCalculoService = horasCalculoService;
        _fichadaExportService = fichadaExportService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fichada>>> GetAll()
    {
        var fichadas = await _fichadaRepository.GetAllAsync();
        return Ok(fichadas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Fichada>> GetById(int id)
    {
        var fichada = await _fichadaRepository.GetByIdAsync(id);
        if (fichada == null)
            return NotFound();

        return Ok(fichada);
    }

    [HttpGet("empleado/{empleadoId}")]
    public async Task<ActionResult<IEnumerable<Fichada>>> GetByEmpleado(int empleadoId)
    {
        var fichadas = await _fichadaRepository.GetByEmpleadoAsync(empleadoId);
        return Ok(fichadas);
    }

    [HttpGet("rango")]
    public async Task<ActionResult<IEnumerable<Fichada>>> GetByFechaRango([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
    {
        var fichadas = await _fichadaRepository.GetByFechaRangoAsync(fechaDesde, fechaHasta);
        return Ok(fichadas);
    }

    [HttpGet("filtros")]
    public async Task<ActionResult<IEnumerable<Fichada>>> GetByFiltros(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? busquedaEmpleado,
        [FromQuery] bool? exportada)
    {
        var fichadas = await _fichadaRepository.GetByFiltrosAsync(fechaDesde, fechaHasta, busquedaEmpleado, exportada);
        return Ok(fichadas);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] Fichada fichada)
    {
        try
        {
            // Calcular horas si tiene empleado, entrada y salida
            if (fichada.EmpleadoId.HasValue &&
                fichada.HoraEntrada.HasValue &&
                fichada.HoraSalida.HasValue)
            {
                var calculo = await _horasCalculoService.CalcularHorasAsync(
                    fichada.EmpleadoId.Value,
                    fichada.HoraEntrada.Value,
                    fichada.HoraSalida.Value);

                fichada.HorasTotales = calculo.HorasTotales;
                fichada.Trabajadas = calculo.HorasTrabajadas;
                fichada.Extras = calculo.HorasExtras;
                fichada.Adicionales = calculo.HorasAdicionales;
            }

            var id = await _fichadaRepository.CreateAsync(fichada);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear fichada", error = ex.Message });
        }
    }

    [HttpPost("importar")]
    public async Task<ActionResult> ImportarFichadas(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { message = "Debe proporcionar un archivo Excel" });
        }

        // Validar extensión
        var extension = Path.GetExtension(archivo.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "El archivo debe ser un Excel (.xlsx o .xls)" });
        }

        using var stream = archivo.OpenReadStream();
        var resultado = await _fichadaImportService.ImportarDesdeExcelAsync(stream);

        if (resultado.TieneErrores)
        {
            return Ok(new
            {
                message = $"Importación completada con advertencias. Importadas: {resultado.FichadasImportadas}, Ignoradas: {resultado.FichadasIgnoradas}",
                fichadasImportadas = resultado.FichadasImportadas,
                fichadasIgnoradas = resultado.FichadasIgnoradas,
                errores = resultado.Errores
            });
        }

        return Ok(new
        {
            message = $"Se importaron {resultado.FichadasImportadas} fichadas correctamente",
            fichadasImportadas = resultado.FichadasImportadas,
            fichadasIgnoradas = resultado.FichadasIgnoradas
        });
    }

    [HttpPost("exportar")]
    public async Task<ActionResult> ExportarFichadas([FromBody] ExportarFichadasRequest request)
    {
        if (request == null || request.IdsFichadas == null || !request.IdsFichadas.Any())
        {
            return BadRequest(new { message = "Debe especificar al menos una fichada para exportar" });
        }

        try
        {
            var resultado = await _fichadaExportService.ExportarFichadasAsync(request.IdsFichadas);

            if (resultado.FichadasExportadas == 0 && resultado.FichadasConError > 0)
            {
                return BadRequest(new
                {
                    message = resultado.Message,
                    fichadasExportadas = resultado.FichadasExportadas,
                    fichadasConError = resultado.FichadasConError,
                    errores = resultado.Errores,
                    advertencias = resultado.Advertencias
                });
            }

            return Ok(new
            {
                message = resultado.Message,
                fichadasExportadas = resultado.FichadasExportadas,
                fichadasConError = resultado.FichadasConError,
                errores = resultado.Errores,
                advertencias = resultado.Advertencias
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al exportar fichadas", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] Fichada fichada)
    {
        try
        {
            // Verificar si la fichada existe y si está exportada
            var fichadaExistente = await _fichadaRepository.GetByIdAsync(id);
            if (fichadaExistente == null)
                return NotFound(new { message = $"Fichada con ID {id} no encontrada" });

            if (fichadaExistente.Exportada)
                return BadRequest(new { message = "No se puede editar una fichada que ya ha sido exportada" });

            fichada.IdFichadas = id;

            // Calcular horas si tiene empleado, entrada y salida
            if (fichada.EmpleadoId.HasValue &&
                fichada.HoraEntrada.HasValue &&
                fichada.HoraSalida.HasValue)
            {
                var calculo = await _horasCalculoService.CalcularHorasAsync(
                    fichada.EmpleadoId.Value,
                    fichada.HoraEntrada.Value,
                    fichada.HoraSalida.Value);

                fichada.HorasTotales = calculo.HorasTotales;
                fichada.Trabajadas = calculo.HorasTrabajadas;
                fichada.Extras = calculo.HorasExtras;
                fichada.Adicionales = calculo.HorasAdicionales;
            }

            var success = await _fichadaRepository.UpdateAsync(fichada);
            if (!success)
                return NotFound();

            await RecalcularFichada(fichada.IdFichadas);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar fichada", error = ex.Message });
        }
    }

    /// <summary>
    /// Recalcula las horas de una fichada específica
    /// </summary>
    [HttpPost("{id}/recalcular")]
    public async Task<ActionResult> RecalcularFichada(int id)
    {
        try
        {
            var fichada = await _fichadaRepository.GetByIdAsync(id);
            if (fichada == null)
                return NotFound(new { message = $"Fichada con ID {id} no encontrada" });

            if (fichada.Exportada)
                return BadRequest(new { message = "No se puede recalcular una fichada que ya ha sido exportada" });

            if (!fichada.EmpleadoId.HasValue)
                return BadRequest(new { message = "La fichada debe tener un empleado asignado" });

            if (!fichada.HoraEntrada.HasValue || !fichada.HoraSalida.HasValue)
                return BadRequest(new { message = "La fichada debe tener hora de entrada y salida" });

            // Calcular las horas
            var calculo = await _horasCalculoService.CalcularHorasAsync(
                fichada.EmpleadoId.Value,
                fichada.HoraEntrada.Value,
                fichada.HoraSalida.Value);

            // Actualizar la fichada con los valores calculados
            fichada.HorasTotales = calculo.HorasTotales;
            fichada.Trabajadas = calculo.HorasTrabajadas;
            fichada.Extras = calculo.HorasExtras;
            fichada.Adicionales = calculo.HorasAdicionales;

            await _fichadaRepository.UpdateAsync(fichada);

            return Ok(new
            {
                message = "Fichada recalculada exitosamente",
                horasTotales = calculo.HorasTotales,
                trabajadas = calculo.HorasTrabajadas,
                extras = calculo.HorasExtras,
                adicionales = calculo.HorasAdicionales,
                advertencias = calculo.Advertencias
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al recalcular fichada", error = ex.Message });
        }
    }

    /// <summary>
    /// Recalcula todas las fichadas no exportadas (solo Admin)
    /// </summary>
    [HttpPost("recalcular-todas")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RecalcularTodasLasFichadas()
    {
        try
        {
            var fichadas = await _fichadaRepository.GetAllAsync();
            int recalculadas = 0;
            int errores = 0;
            int ignoradas = 0;
            var mensajesError = new List<string>();

            foreach (var fichada in fichadas)
            {
                try
                {
                    // Ignorar fichadas exportadas
                    if (fichada.Exportada)
                    {
                        ignoradas++;
                        continue;
                    }

                    if (!fichada.EmpleadoId.HasValue)
                    {
                        errores++;
                        mensajesError.Add($"Fichada ID {fichada.IdFichadas}: No tiene empleado asignado");
                        continue;
                    }

                    if (!fichada.HoraEntrada.HasValue || !fichada.HoraSalida.HasValue)
                    {
                        errores++;
                        mensajesError.Add($"Fichada ID {fichada.IdFichadas}: No tiene hora de entrada/salida");
                        continue;
                    }

                    var calculo = await _horasCalculoService.CalcularHorasAsync(
                        fichada.EmpleadoId.Value,
                        fichada.HoraEntrada.Value,
                        fichada.HoraSalida.Value);

                    fichada.HorasTotales = calculo.HorasTotales;
                    fichada.Trabajadas = calculo.HorasTrabajadas;
                    fichada.Extras = calculo.HorasExtras;
                    fichada.Adicionales = calculo.HorasAdicionales;

                    await _fichadaRepository.UpdateAsync(fichada);
                    recalculadas++;

                    // Agregar advertencias si las hay
                    if (calculo.Advertencias.Any())
                    {
                        foreach (var advertencia in calculo.Advertencias)
                        {
                            mensajesError.Add($"Fichada ID {fichada.IdFichadas}: {advertencia}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errores++;
                    mensajesError.Add($"Fichada ID {fichada.IdFichadas}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = $"Recálculo completado. Recalculadas: {recalculadas}, Ignoradas (exportadas): {ignoradas}, Errores: {errores}",
                recalculadas,
                ignoradas,
                errores,
                advertencias = mensajesError
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al recalcular fichadas", error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga las fichadas filtradas en formato Excel
    /// </summary>
    [HttpGet("descargar-excel")]
    public async Task<ActionResult> DescargarExcel(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? busquedaEmpleado,
        [FromQuery] bool? exportada)
    {
        try
        {
            // Obtener fichadas con los filtros aplicados
            var fichadas = await _fichadaRepository.GetByFiltrosAsync(fechaDesde, fechaHasta, busquedaEmpleado, exportada);

            if (!fichadas.Any())
            {
                return BadRequest(new { message = "No se encontraron fichadas con los filtros especificados" });
            }

            // Crear el archivo Excel usando EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Fichadas");

            // Encabezados
            worksheet.Cells[1, 1].Value = "Legajo";
            worksheet.Cells[1, 2].Value = "Empleado";
            worksheet.Cells[1, 3].Value = "Sector";
            worksheet.Cells[1, 4].Value = "Fecha";
            worksheet.Cells[1, 5].Value = "Hora Entrada";
            worksheet.Cells[1, 6].Value = "Hora Salida";
            worksheet.Cells[1, 7].Value = "Total (hs)";
            worksheet.Cells[1, 8].Value = "Trabajadas (hs)";
            worksheet.Cells[1, 9].Value = "Extras (hs)";
            worksheet.Cells[1, 10].Value = "Adicionales (hs)";
            worksheet.Cells[1, 11].Value = "Novedad";
            worksheet.Cells[1, 12].Value = "Estado";

            // Estilo del encabezado
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Datos
            int row = 2;
            foreach (var fichada in fichadas)
            {
                worksheet.Cells[row, 1].Value = fichada.EmpleadoLegajo;
                worksheet.Cells[row, 2].Value = fichada.EmpleadoNombre;
                worksheet.Cells[row, 3].Value = fichada.SectorNombre;
                worksheet.Cells[row, 4].Value = fichada.HoraEntrada?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 5].Value = fichada.HoraEntrada?.ToString("HH:mm");
                worksheet.Cells[row, 6].Value = fichada.HoraSalida?.ToString("HH:mm");
                worksheet.Cells[row, 7].Value = fichada.HorasTotales.HasValue ? Math.Round(fichada.HorasTotales.Value / 60.0, 2) : (double?)null;
                worksheet.Cells[row, 8].Value = fichada.Trabajadas.HasValue ? Math.Round(fichada.Trabajadas.Value / 60.0, 2) : (double?)null;
                worksheet.Cells[row, 9].Value = fichada.Extras.HasValue ? Math.Round(fichada.Extras.Value / 60.0, 2) : (double?)null;
                worksheet.Cells[row, 10].Value = fichada.Adicionales.HasValue ? Math.Round(fichada.Adicionales.Value / 60.0, 2) : (double?)null;
                worksheet.Cells[row, 11].Value = !string.IsNullOrEmpty(fichada.NovedadDescripcion)
                    ? $"{fichada.NovedadCodigo} - {fichada.NovedadDescripcion}"
                    : "-";
                worksheet.Cells[row, 12].Value = fichada.Exportada == true ? "Exportada" : "Pendiente";
                row++;
            }

            // Fila de totales
            worksheet.Cells[row, 1].Value = "TOTALES";
            worksheet.Cells[row, 1, row, 3].Merge = true;
            worksheet.Cells[row, 7].Formula = $"SUM(G2:G{row - 1})";
            worksheet.Cells[row, 8].Formula = $"SUM(H2:H{row - 1})";
            worksheet.Cells[row, 9].Formula = $"SUM(I2:I{row - 1})";
            worksheet.Cells[row, 10].Formula = $"SUM(J2:J{row - 1})";

            // Estilo de totales
            using (var range = worksheet.Cells[row, 1, row, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Ajustar ancho de columnas
            worksheet.Cells.AutoFitColumns();

            // Generar el archivo
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Fichadas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar archivo Excel", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        // Verificar si la fichada existe y si está exportada
        var fichada = await _fichadaRepository.GetByIdAsync(id);
        if (fichada == null)
            return NotFound(new { message = $"Fichada con ID {id} no encontrada" });

        if (fichada.Exportada)
            return BadRequest(new { message = "No se puede eliminar una fichada que ya ha sido exportada" });

        var success = await _fichadaRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
