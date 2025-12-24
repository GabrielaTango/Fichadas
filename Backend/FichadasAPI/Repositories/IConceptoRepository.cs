using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public interface IConceptoRepository
{
    Task<IEnumerable<Concepto>> GetAllAsync();
    Task<Concepto?> GetByIdAsync(int id);
    Task<Concepto?> GetByIdConceptoTangoAsync(int idConceptoTango);
    Task<Concepto?> GetByNroConceptoAsync(int nroConcepto);
    Task<int> CreateAsync(Concepto concepto);
    Task<bool> UpdateAsync(Concepto concepto);
    Task<bool> DeleteAsync(int id);
    Task<ImportarConceptosResult> ImportarDesdeTangoAsync();
    Task<IEnumerable<ConceptoTango>> GetConceptosDesdeTangoAsync();
    Task<bool> ImportarConceptoEspecificoAsync(int idConceptoTango);
}
