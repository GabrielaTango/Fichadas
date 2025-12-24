namespace FichadasAPI.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string? Nombre { get; set; }
    public int? Legajo { get; set; }
    public int? SectorId { get; set; }
    public string? SectorNombre { get; set; }
    public TimeSpan? HorarioEntrada { get; set; }
    public TimeSpan? HorarioSalida { get; set; }
    public DateTime? FechaInicioRotacion { get; set; }
}

public class EmpleadoTango
{
    public int NroLegajo { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ImportarEmpleadosResult
{
    public int EmpleadosImportados { get; set; }
    public int EmpleadosExistentes { get; set; }
    public int EmpleadosActualizados { get; set; }
    public List<string> Errores { get; set; } = new();
    public bool TieneErrores => Errores.Count > 0;
}
