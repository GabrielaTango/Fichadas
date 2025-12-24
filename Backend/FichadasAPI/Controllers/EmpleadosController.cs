using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoRepository _empleadoRepository;

    public EmpleadosController(IEmpleadoRepository empleadoRepository)
    {
        _empleadoRepository = empleadoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetAll()
    {
        var empleados = await _empleadoRepository.GetAllAsync();
        return Ok(empleados);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empleado>> GetById(int id)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return NotFound();

        return Ok(empleado);
    }

    [HttpGet("legajo/{legajo}")]
    public async Task<ActionResult<Empleado>> GetByLegajo(int legajo)
    {
        var empleado = await _empleadoRepository.GetByLegajoAsync(legajo);
        if (empleado == null)
            return NotFound();

        return Ok(empleado);
    }

    [HttpGet("sector/{sectorId}")]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetBySector(int sectorId)
    {
        var empleados = await _empleadoRepository.GetBySectorAsync(sectorId);
        return Ok(empleados);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] Empleado empleado)
    {
        var id = await _empleadoRepository.CreateAsync(empleado);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, [FromBody] Empleado empleado)
    {
        empleado.IdEmpleado = id;
        var success = await _empleadoRepository.UpdateAsync(empleado);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _empleadoRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Importa empleados desde la base de datos de Tango (DELTA3.DBO.LEGAJO)
    /// Los empleados se crean sin sector asignado (sector_id = NULL)
    /// </summary>
    [HttpPost("importar-desde-tango")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportarDesdeTango()
    {
        try
        {
            var resultado = await _empleadoRepository.ImportarDesdeTangoAsync();

            if (resultado.TieneErrores)
            {
                return Ok(new
                {
                    message = $"Importación completada con advertencias. Nuevos: {resultado.EmpleadosImportados}, Actualizados: {resultado.EmpleadosActualizados}, Existentes: {resultado.EmpleadosExistentes}",
                    empleadosImportados = resultado.EmpleadosImportados,
                    empleadosActualizados = resultado.EmpleadosActualizados,
                    empleadosExistentes = resultado.EmpleadosExistentes,
                    errores = resultado.Errores
                });
            }

            return Ok(new
            {
                message = $"Importación exitosa. Nuevos: {resultado.EmpleadosImportados}, Actualizados: {resultado.EmpleadosActualizados}, Existentes: {resultado.EmpleadosExistentes}",
                empleadosImportados = resultado.EmpleadosImportados,
                empleadosActualizados = resultado.EmpleadosActualizados,
                empleadosExistentes = resultado.EmpleadosExistentes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar empleados desde Tango", error = ex.Message });
        }
    }
}
