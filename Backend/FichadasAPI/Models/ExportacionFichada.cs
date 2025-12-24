namespace FichadasAPI.Models;

public class ExportarFichadasRequest
{
    public List<int> IdsFichadas { get; set; } = new();
}

public class ExportarFichadasResult
{
    public int FichadasExportadas { get; set; }
    public int FichadasConError { get; set; }
    public List<string> Errores { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public bool TieneErrores => Errores.Count > 0;
    public string Message { get; set; } = string.Empty;
}

public class NovedadRegistrada
{
    public int IdLegajo { get; set; }
    public int IdNovedad { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Cantidad { get; set; }
    public string OrigenCloud { get; set; } = "Externo";
    public string OrigenNovedad { get; set; } = "Externo";
}
