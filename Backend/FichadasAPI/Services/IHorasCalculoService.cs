using FichadasAPI.Models;

namespace FichadasAPI.Services;

public interface IHorasCalculoService
{
    /// <summary>
    /// Calcula las horas trabajadas, extras y adicionales para una fichada.
    /// Usa la configuración activa del sector del empleado.
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="horaEntrada">Hora de entrada</param>
    /// <param name="horaSalida">Hora de salida</param>
    /// <returns>Resultado del cálculo con horas trabajadas, extras y adicionales</returns>
    Task<ResultadoCalculoHoras> CalcularHorasAsync(int empleadoId, DateTime horaEntrada, DateTime horaSalida);
}

/// <summary>
/// Resultado del cálculo de horas
/// </summary>
public class ResultadoCalculoHoras
{
    /// <summary>
    /// Total de horas trabajadas (en minutos)
    /// </summary>
    public int HorasTotales { get; set; }

    /// <summary>
    /// Horas normales trabajadas (en minutos)
    /// </summary>
    public int HorasTrabajadas { get; set; }

    /// <summary>
    /// Horas extras oficiales (en minutos)
    /// </summary>
    public int HorasExtras { get; set; }

    /// <summary>
    /// Horas extras adicionales (en minutos)
    /// </summary>
    public int HorasAdicionales { get; set; }

    /// <summary>
    /// Minutos de descuento por llegada tarde
    /// </summary>
    public int MinutosDescuento { get; set; }

    /// <summary>
    /// Minutos de llegada tarde
    /// </summary>
    public int MinutosTarde { get; set; }

    /// <summary>
    /// Configuración utilizada para el cálculo
    /// </summary>
    public ConfiguracionCalculo? ConfiguracionUtilizada { get; set; }

    /// <summary>
    /// Mensajes de advertencia o información sobre el cálculo
    /// </summary>
    public List<string> Advertencias { get; set; } = new();
}
