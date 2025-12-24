using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionCalculoController : ControllerBase
{
    private readonly IConfiguracionCalculoRepository _repository;
    private readonly ISectorRepository _sectorRepository;

    public ConfiguracionCalculoController(
        IConfiguracionCalculoRepository repository,
        ISectorRepository sectorRepository)
    {
        _repository = repository;
        _sectorRepository = sectorRepository;
    }

    /// <summary>
    /// Obtiene todas las configuraciones de cálculo
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfiguracionCalculo>>> GetAll()
    {
        try
        {
            var configuraciones = await _repository.GetAllAsync();
            return Ok(configuraciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener configuraciones", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una configuración por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConfiguracionCalculo>> GetById(int id)
    {
        try
        {
            var configuracion = await _repository.GetByIdAsync(id);
            if (configuracion == null)
                return NotFound(new { message = $"Configuración con ID {id} no encontrada" });

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener configuración", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene configuraciones por sector
    /// </summary>
    [HttpGet("sector/{sectorId}")]
    public async Task<ActionResult<IEnumerable<ConfiguracionCalculo>>> GetBySector(int sectorId)
    {
        try
        {
            // Verificar que el sector existe
            var sector = await _sectorRepository.GetByIdAsync(sectorId);
            if (sector == null)
                return NotFound(new { message = $"Sector con ID {sectorId} no encontrado" });

            var configuraciones = await _repository.GetBySectorAsync(sectorId);
            return Ok(configuraciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener configuraciones", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene la configuración activa para un sector y temporada
    /// </summary>
    [HttpGet("sector/{sectorId}/temporada/{esVerano}")]
    public async Task<ActionResult<ConfiguracionCalculo>> GetBySectorYTemporada(int sectorId, bool esVerano)
    {
        try
        {
            var configuracion = await _repository.GetBySectorYTemporadaAsync(sectorId, esVerano);
            if (configuracion == null)
                return NotFound(new { message = $"No hay configuración activa para sector {sectorId} en temporada {(esVerano ? "verano" : "invierno")}" });

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener configuración", error = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva configuración de cálculo
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ConfiguracionCalculo>> Create([FromBody] ConfiguracionCalculo configuracion)
    {
        try
        {
            // Validaciones
            if (configuracion.SectorId <= 0)
                return BadRequest(new { message = "El ID de sector es requerido" });

            // Verificar que el sector existe
            var sector = await _sectorRepository.GetByIdAsync(configuracion.SectorId);
            if (sector == null)
                return NotFound(new { message = $"Sector con ID {configuracion.SectorId} no encontrado" });

            if (configuracion.HorasNormales <= 0)
                return BadRequest(new { message = "Las horas normales deben ser mayores a 0" });

            if (configuracion.ToleranciaMinutos < 0)
                return BadRequest(new { message = "La tolerancia no puede ser negativa" });

            var id = await _repository.CreateAsync(configuracion);
            configuracion.IdConfiguracion = id;

            return CreatedAtAction(nameof(GetById), new { id }, configuracion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear configuración", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una configuración existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, [FromBody] ConfiguracionCalculo configuracion)
    {
        try
        {
            // Validaciones
            if (id != configuracion.IdConfiguracion)
                return BadRequest(new { message = "El ID de la URL no coincide con el ID de la configuración" });

            var existente = await _repository.GetByIdAsync(id);
            if (existente == null)
                return NotFound(new { message = $"Configuración con ID {id} no encontrada" });

            // Verificar que el sector existe
            var sector = await _sectorRepository.GetByIdAsync(configuracion.SectorId);
            if (sector == null)
                return NotFound(new { message = $"Sector con ID {configuracion.SectorId} no encontrado" });

            if (configuracion.HorasNormales <= 0)
                return BadRequest(new { message = "Las horas normales deben ser mayores a 0" });

            if (configuracion.ToleranciaMinutos < 0)
                return BadRequest(new { message = "La tolerancia no puede ser negativa" });

            var resultado = await _repository.UpdateAsync(configuracion);
            if (!resultado)
                return StatusCode(500, new { message = "No se pudo actualizar la configuración" });

            return Ok(new { message = "Configuración actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar configuración", error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una configuración
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var configuracion = await _repository.GetByIdAsync(id);
            if (configuracion == null)
                return NotFound(new { message = $"Configuración con ID {id} no encontrada" });

            var resultado = await _repository.DeleteAsync(id);
            if (!resultado)
                return StatusCode(500, new { message = "No se pudo eliminar la configuración" });

            return Ok(new { message = "Configuración eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar configuración", error = ex.Message });
        }
    }
}
