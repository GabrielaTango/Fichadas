using FichadasAPI.Models;
using FichadasAPI.Repositories;
using FichadasAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichadasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuthService _authService;

    public UsuariosController(IUsuarioRepository usuarioRepository, IAuthService authService)
    {
        _usuarioRepository = usuarioRepository;
        _authService = authService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        var usuariosDto = usuarios.Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario,
            Usuario = u.UsuarioNombre,
            Mail = u.Mail,
            EsAdmin = u.EsAdmin.GetValueOrDefault()
        });
        return Ok(usuariosDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        var usuarioDto = new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario,
            Usuario = usuario.UsuarioNombre,
            Mail = usuario.Mail,
            EsAdmin = usuario.EsAdmin.GetValueOrDefault()
        };

        return Ok(usuarioDto);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateUsuarioRequest request)
    {
        // Validar que el usuario no exista
        var existingUser = await _usuarioRepository.GetByUsuarioAsync(request.Usuario);
        if (existingUser != null)
            return BadRequest(new { message = "El nombre de usuario ya existe" });

        // Hashear la contraseña
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var usuario = new Usuario
        {
            UsuarioNombre = request.Usuario,
            Password = hashedPassword,
            Mail = request.Mail,
            EsAdmin = request.EsAdmin
        };

        var id = await _usuarioRepository.CreateAsync(usuario);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        // Validar que el nuevo nombre de usuario no exista (si cambió)
        if (usuario.UsuarioNombre != request.Usuario)
        {
            var existingUser = await _usuarioRepository.GetByUsuarioAsync(request.Usuario);
            if (existingUser != null)
                return BadRequest(new { message = "El nombre de usuario ya existe" });
        }

        usuario.UsuarioNombre = request.Usuario;
        usuario.Mail = request.Mail;
        usuario.EsAdmin = request.EsAdmin;

        var success = await _usuarioRepository.UpdateAsync(usuario);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        // IMPORTANTE: No permitir eliminar el usuario admin
        if (usuario.UsuarioNombre.ToLower() == "admin")
            return BadRequest(new { message = "No se puede eliminar el usuario admin" });

        var success = await _usuarioRepository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, [FromBody] AdminChangePasswordRequest request)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        // Hashear la nueva contraseña
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Actualizar la contraseña directamente (sin verificar la anterior porque es admin)
        var success = await _usuarioRepository.UpdatePasswordAsync(id, hashedPassword);

        if (!success)
            return BadRequest(new { message = "No se pudo cambiar la contraseña" });

        return Ok(new { message = "Contraseña actualizada correctamente" });
    }
}

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
}

public class CreateUsuarioRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
}

public class UpdateUsuarioRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
}

public class AdminChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
