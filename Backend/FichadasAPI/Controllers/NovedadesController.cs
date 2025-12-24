using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NovedadesController : ControllerBase
{
    private readonly INovedadRepository _novedadRepository;

    public NovedadesController(INovedadRepository novedadRepository)
    {
        _novedadRepository = novedadRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Novedad>>> GetAll()
    {
        var novedades = await _novedadRepository.GetAllAsync();
        return Ok(novedades);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Novedad>> GetById(int id)
    {
        var novedad = await _novedadRepository.GetByIdAsync(id);
        if (novedad == null)
            return NotFound();

        return Ok(novedad);
    }

    [HttpGet("codigo/{codNovedad}")]
    public async Task<ActionResult<Novedad>> GetByCodNovedad(string codNovedad)
    {
        var novedad = await _novedadRepository.GetByCodNovedadAsync(codNovedad);
        if (novedad == null)
            return NotFound();

        return Ok(novedad);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] Novedad novedad)
    {
        var id = await _novedadRepository.CreateAsync(novedad);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, [FromBody] Novedad novedad)
    {
        novedad.IdNovedad = id;
        var success = await _novedadRepository.UpdateAsync(novedad);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _novedadRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Importa novedades desde la base de datos de Tango (NOVEDAD)
    /// </summary>
    [HttpPost("importar-desde-tango")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportarDesdeTango()
    {
        try
        {
            var resultado = await _novedadRepository.ImportarDesdeTangoAsync();

            if (resultado.TieneErrores)
            {
                return Ok(new
                {
                    message = $"Importación completada con advertencias. Nuevas: {resultado.NovedadesImportadas}, Actualizadas: {resultado.NovedadesActualizadas}, Existentes: {resultado.NovedadesExistentes}",
                    novedadesImportadas = resultado.NovedadesImportadas,
                    novedadesActualizadas = resultado.NovedadesActualizadas,
                    novedadesExistentes = resultado.NovedadesExistentes,
                    errores = resultado.Errores
                });
            }

            return Ok(new
            {
                message = $"Importación exitosa. Nuevas: {resultado.NovedadesImportadas}, Actualizadas: {resultado.NovedadesActualizadas}, Existentes: {resultado.NovedadesExistentes}",
                novedadesImportadas = resultado.NovedadesImportadas,
                novedadesActualizadas = resultado.NovedadesActualizadas,
                novedadesExistentes = resultado.NovedadesExistentes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar novedades desde Tango", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene novedades disponibles desde Tango (sin importar)
    /// </summary>
    [HttpGet("disponibles-tango")]
    public async Task<ActionResult> GetNovedadesDesdeTango()
    {
        try
        {
            var novedadesTango = await _novedadRepository.GetNovedadesDesdeTangoAsync();
            return Ok(novedadesTango);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener novedades de Tango", error = ex.Message });
        }
    }

    /// <summary>
    /// Importa una novedad específica desde Tango
    /// </summary>
    [HttpPost("importar-novedad/{idNovedadTango}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportarNovedadEspecifica(int idNovedadTango)
    {
        try
        {
            var success = await _novedadRepository.ImportarNovedadEspecificaAsync(idNovedadTango);

            if (!success)
            {
                return BadRequest(new { message = "La novedad ya existe o no se pudo importar" });
            }

            return Ok(new { message = "Novedad importada exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar novedad", error = ex.Message });
        }
    }
}
