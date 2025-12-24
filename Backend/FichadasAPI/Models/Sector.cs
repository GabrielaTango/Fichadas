namespace FichadasAPI.Models;

public class Sector
{
    public int IdSector { get; set; }
    public string? Nombre { get; set; }
    public bool EsRotativo { get; set; }
    public int? NovedadExtrasId { get; set; }
    public int? NovedadTrabajadasId { get; set; }

    // Campos adicionales para JOIN
    public string? NovedadExtrasCodigo { get; set; }
    public string? NovedadExtrasDescripcion { get; set; }
    public string? NovedadTrabajadasCodigo { get; set; }
    public string? NovedadTrabajadasDescripcion { get; set; }
}
