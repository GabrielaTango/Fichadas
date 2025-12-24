using FichadasAPI.Models;

namespace FichadasAPI.Services;

public interface IFichadaExportService
{
    Task<ExportarFichadasResult> ExportarFichadasAsync(List<int> idsFichadas);
}
