using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SectoresController : ControllerBase
{
    private readonly ISectorRepository _sectorRepository;

    public SectoresController(ISectorRepository sectorRepository)
    {
        _sectorRepository = sectorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sector>>> GetAll()
    {
        var sectores = await _sectorRepository.GetAllAsync();
        return Ok(sectores);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sector>> GetById(int id)
    {
        var sector = await _sectorRepository.GetByIdAsync(id);
        if (sector == null)
            return NotFound();

        return Ok(sector);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] Sector sector)
    {
        var id = await _sectorRepository.CreateAsync(sector);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, [FromBody] Sector sector)
    {
        sector.IdSector = id;
        var success = await _sectorRepository.UpdateAsync(sector);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _sectorRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
