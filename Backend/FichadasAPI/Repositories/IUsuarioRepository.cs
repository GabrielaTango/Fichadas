using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByUsuarioAsync(string usuario);
    Task<Usuario?> GetByIdAsync(int id);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<int> CreateAsync(Usuario usuario);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdatePasswordAsync(int id, string hashedPassword);
}
