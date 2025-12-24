using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConceptosController : ControllerBase
{
    private readonly IConceptoRepository _conceptoRepository;

    public ConceptosController(IConceptoRepository conceptoRepository)
    {
        _conceptoRepository = conceptoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Concepto>>> GetAll()
    {
        var conceptos = await _conceptoRepository.GetAllAsync();
        return Ok(conceptos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Concepto>> GetById(int id)
    {
        var concepto = await _conceptoRepository.GetByIdAsync(id);
        if (concepto == null)
            return NotFound();

        return Ok(concepto);
    }

    [HttpGet("nro-concepto/{nroConcepto}")]
    public async Task<ActionResult<Concepto>> GetByNroConcepto(int nroConcepto)
    {
        var concepto = await _conceptoRepository.GetByNroConceptoAsync(nroConcepto);
        if (concepto == null)
            return NotFound();

        return Ok(concepto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] Concepto concepto)
    {
        var id = await _conceptoRepository.CreateAsync(concepto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, [FromBody] Concepto concepto)
    {
        concepto.IdConcepto = id;
        var success = await _conceptoRepository.UpdateAsync(concepto);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _conceptoRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Importa conceptos desde la base de datos de Tango (CONCEPTO)
    /// </summary>
    [HttpPost("importar-desde-tango")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportarDesdeTango()
    {
        try
        {
            var resultado = await _conceptoRepository.ImportarDesdeTangoAsync();

            if (resultado.TieneErrores)
            {
                return Ok(new
                {
                    message = $"Importación completada con advertencias. Nuevos: {resultado.ConceptosImportados}, Actualizados: {resultado.ConceptosActualizados}, Existentes: {resultado.ConceptosExistentes}",
                    conceptosImportados = resultado.ConceptosImportados,
                    conceptosActualizados = resultado.ConceptosActualizados,
                    conceptosExistentes = resultado.ConceptosExistentes,
                    errores = resultado.Errores
                });
            }

            return Ok(new
            {
                message = $"Importación exitosa. Nuevos: {resultado.ConceptosImportados}, Actualizados: {resultado.ConceptosActualizados}, Existentes: {resultado.ConceptosExistentes}",
                conceptosImportados = resultado.ConceptosImportados,
                conceptosActualizados = resultado.ConceptosActualizados,
                conceptosExistentes = resultado.ConceptosExistentes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar conceptos desde Tango", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene conceptos disponibles desde Tango (sin importar)
    /// </summary>
    [HttpGet("disponibles-tango")]
    public async Task<ActionResult> GetConceptosDesdeTango()
    {
        try
        {
            var conceptosTango = await _conceptoRepository.GetConceptosDesdeTangoAsync();
            return Ok(conceptosTango);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener conceptos de Tango", error = ex.Message });
        }
    }

    /// <summary>
    /// Importa un concepto específico desde Tango
    /// </summary>
    [HttpPost("importar-concepto/{idConceptoTango}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportarConceptoEspecifico(int idConceptoTango)
    {
        try
        {
            var success = await _conceptoRepository.ImportarConceptoEspecificoAsync(idConceptoTango);

            if (!success)
            {
                return BadRequest(new { message = "El concepto ya existe o no se pudo importar" });
            }

            return Ok(new { message = "Concepto importado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar concepto", error = ex.Message });
        }
    }
}
