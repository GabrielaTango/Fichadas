using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface IEmpleadoRepository
{
    Task<IEnumerable<Empleado>> GetAllAsync();
    Task<Empleado?> GetByIdAsync(int id);
    Task<Empleado?> GetByLegajoAsync(int legajo);
    Task<IEnumerable<Empleado>> GetBySectorAsync(int sectorId);
    Task<int> CreateAsync(Empleado empleado);
    Task<bool> UpdateAsync(Empleado empleado);
    Task<bool> DeleteAsync(int id);
    Task<ImportarEmpleadosResult> ImportarDesdeTangoAsync();
}
