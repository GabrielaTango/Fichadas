using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface IFichadaRepository
{
    Task<IEnumerable<Fichada>> GetAllAsync();
    Task<Fichada?> GetByIdAsync(int id);
    Task<IEnumerable<Fichada>> GetByEmpleadoAsync(int empleadoId);
    Task<IEnumerable<Fichada>> GetByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta);
    Task<IEnumerable<Fichada>> GetByFiltrosAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? busquedaEmpleado, bool? exportada);
    Task<int> CreateAsync(Fichada fichada);
    Task<bool> UpdateAsync(Fichada fichada);
    Task<bool> DeleteAsync(int id);
    Task<int> ImportarFichadasAsync(List<FichadaExcel> fichadas);
}
