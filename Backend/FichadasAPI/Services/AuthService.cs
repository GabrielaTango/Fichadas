using FichadasAPI.Models;
using FichadasAPI.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FichadasAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.GetByUsuarioAsync(request.Usuario);
        if (usuario == null)
            return null;

        // Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
            return null;

        var token = GenerateJwtToken(usuario);

        return new LoginResponse
        {
            IdUsuario = usuario.IdUsuario,
            Usuario = usuario.UsuarioNombre ?? string.Empty,
            Mail = usuario.Mail ?? string.Empty,
            EsAdmin = usuario.EsAdmin ?? false,
            Token = token
        };
    }

    public string GenerateJwtToken(Usuario usuario)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("Secret key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.UsuarioNombre ?? string.Empty),
            new Claim(ClaimTypes.Email, usuario.Mail ?? string.Empty),
            new Claim(ClaimTypes.Role, usuario.EsAdmin == true ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.Password))
            return false;

        usuario.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _usuarioRepository.UpdateAsync(usuario);
    }
}
