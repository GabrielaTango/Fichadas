namespace FichadasAPI.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? Password { get; set; }
    public string? Mail { get; set; }
    public bool? EsAdmin { get; set; }
}

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public int IdUsuario { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
    public string Token { get; set; } = string.Empty;
}
