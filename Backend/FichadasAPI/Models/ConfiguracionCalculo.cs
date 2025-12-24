namespace FichadasAPI.Models;

/// <summary>
/// Configuración de reglas de cálculo de horas por sector y temporada
/// </summary>
public class ConfiguracionCalculo
{
    public int IdConfiguracion { get; set; }
    public int SectorId { get; set; }
    public bool EsVerano { get; set; }

    // Horas normales esperadas
    public int HorasNormales { get; set; }

    // Horas extras oficiales (generalmente 1 hora)
    public int HorasExtrasOficiales { get; set; }

    // Horas extras adicionales (varía por sector y temporada)
    public int HorasExtrasAdicionales { get; set; }

    // Tolerancia de llegada tarde (generalmente 5 minutos)
    public int ToleranciaMinutos { get; set; }

    // Descuento si llega tarde entre 6-30 minutos
    public int DescuentoTarde6a30Min { get; set; }

    // Descuento si llega tarde 31+ minutos
    public int DescuentoTarde31Mas { get; set; }

    // Horarios de referencia (opcional, para validaciones)
    public TimeSpan? HoraEntradaEsperada { get; set; }
    public TimeSpan? HoraSalidaEsperada { get; set; }

    // Tipo de turno para sectores rotativos: "diurno" o "nocturno"
    // NULL para sectores no rotativos
    public string? TipoTurno { get; set; }

    // Solo una configuración puede estar activa por sector/temporada
    // Para sectores rotativos pueden haber 2 activas (una diurna y una nocturna)
    public bool Activo { get; set; }

    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
