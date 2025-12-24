using FichadasAPI.Models;

namespace FichadasAPI.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    string GenerateJwtToken(Usuario usuario);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
