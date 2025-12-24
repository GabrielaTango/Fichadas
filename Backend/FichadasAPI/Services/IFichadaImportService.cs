using FichadasAPI.Models;

namespace FichadasAPI.Services;

public interface IFichadaImportService
{
    Task<FichadaImportResult> ImportarDesdeExcelAsync(Stream excelStream);
}

public class FichadaImportResult
{
    public int FichadasImportadas { get; set; }
    public int FichadasIgnoradas { get; set; }
    public List<string> Errores { get; set; } = new();
    public bool TieneErrores => Errores.Any();
}
