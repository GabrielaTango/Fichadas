using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface ISectorRepository
{
    Task<IEnumerable<Sector>> GetAllAsync();
    Task<Sector?> GetByIdAsync(int id);
    Task<int> CreateAsync(Sector sector);
    Task<bool> UpdateAsync(Sector sector);
    Task<bool> DeleteAsync(int id);
}
