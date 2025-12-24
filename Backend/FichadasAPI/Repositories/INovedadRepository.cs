using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface INovedadRepository
{
    Task<IEnumerable<Novedad>> GetAllAsync();
    Task<Novedad?> GetByIdAsync(int id);
    Task<Novedad?> GetByCodNovedadAsync(string codNovedad);
    Task<int> CreateAsync(Novedad novedad);
    Task<bool> UpdateAsync(Novedad novedad);
    Task<bool> DeleteAsync(int id);
    Task<ImportarNovedadesResult> ImportarDesdeTangoAsync();
    Task<IEnumerable<NovedadTango>> GetNovedadesDesdeTangoAsync();
    Task<bool> ImportarNovedadEspecificaAsync(int idNovedadTango);
}
