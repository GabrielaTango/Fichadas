namespace FichadasAPI.Models;

public class Novedad
{
    public int IdNovedad { get; set; }
    public string CodNovedad { get; set; } = string.Empty;
    public string DescNovedad { get; set; } = string.Empty;
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class NovedadTango
{
    public int IdNovedad { get; set; }
    public string CodNovedad { get; set; } = string.Empty;
    public string DescNovedad { get; set; } = string.Empty;
}

public class ImportarNovedadesResult
{
    public int NovedadesImportadas { get; set; }
    public int NovedadesExistentes { get; set; }
    public int NovedadesActualizadas { get; set; }
    public List<string> Errores { get; set; } = new();
    public bool TieneErrores => Errores.Count > 0;
}
