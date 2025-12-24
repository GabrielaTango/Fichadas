using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface IConfiguracionCalculoRepository
{
    Task<IEnumerable<ConfiguracionCalculo>> GetAllAsync();
    Task<ConfiguracionCalculo?> GetByIdAsync(int id);
    Task<ConfiguracionCalculo?> GetBySectorYTemporadaAsync(int sectorId, bool esVerano);
    Task<ConfiguracionCalculo?> GetActivaBySectorAsync(int sectorId);
    Task<ConfiguracionCalculo?> GetBySectorTipoTurnoAsync(int sectorId, string tipoTurno);
    Task<IEnumerable<ConfiguracionCalculo>> GetBySectorAsync(int sectorId);
    Task<int> CreateAsync(ConfiguracionCalculo configuracion);
    Task<bool> UpdateAsync(ConfiguracionCalculo configuracion);
    Task<bool> DeleteAsync(int id);
}
