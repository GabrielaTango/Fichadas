namespace FichadasAPI.Models;

public class Concepto
{
    public int IdConcepto { get; set; }
    public int IdConceptoTango { get; set; }
    public int NroConcepto { get; set; }
    public string DescConcepto { get; set; } = string.Empty;
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class ConceptoTango
{
    public int IdConcepto { get; set; }
    public int NroConcepto { get; set; }
    public string DescConcepto { get; set; } = string.Empty;
}

public class ImportarConceptosResult
{
    public int ConceptosImportados { get; set; }
    public int ConceptosExistentes { get; set; }
    public int ConceptosActualizados { get; set; }
    public List<string> Errores { get; set; } = new();
    public bool TieneErrores => Errores.Count > 0;
}
