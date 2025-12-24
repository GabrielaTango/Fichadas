namespace FichadasAPI.Models;

public class Fichada
{
    public int IdFichadas { get; set; }
    public int? EmpleadoId { get; set; }
    public DateTime? HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; }
    public int? HorarioTurnoId { get; set; }
    public int? HorasTotales { get; set; }
    public int? Trabajadas { get; set; }
    public int? Extras { get; set; }
    public int? Adicionales { get; set; }
    public string? CodigoNovedad { get; set; }
    public int? NovedadId { get; set; }
    public bool Exportada { get; set; }
    public DateTime? FechaExportacion { get; set; }
    // Campos adicionales para JOIN
    public string? NovedadDescripcion { get; set; }
    public string? NovedadCodigo { get; set; }
    public int? EmpleadoLegajo { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? EmpleadoIdTango { get; set; }
    public int? NovedadIdTango { get; set; }
    public string? SectorNombre { get; set; }
    public int? SectorId { get; set; }
    public int? SectorNovedadExtrasId { get; set; }
    public string? NovedadExtrasCodigo { get; set; }
    public int? NovedadExtrasIdTango { get; set; }
}

public class FichadaImportRequest
{
    public List<FichadaExcel> Fichadas { get; set; } = new();
}

public class FichadaExcel
{
    public int Legajo { get; set; }
    public DateTime FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Entrada" o "Salida"
}
