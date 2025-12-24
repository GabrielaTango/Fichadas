using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class ConfiguracionCalculoRepository : IConfiguracionCalculoRepository
{
    private readonly DapperContext _context;

    public ConfiguracionCalculoRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConfiguracionCalculo>> GetAllAsync()
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionCalculo>(query);
    }

    public async Task<ConfiguracionCalculo?> GetByIdAsync(int id)
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo
                      WHERE id_configuracion = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ConfiguracionCalculo>(query, new { Id = id });
    }

    public async Task<ConfiguracionCalculo?> GetBySectorYTemporadaAsync(int sectorId, bool esVerano)
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo
                      WHERE sector_id = @SectorId AND es_verano = @EsVerano AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ConfiguracionCalculo>(query,
            new { SectorId = sectorId, EsVerano = esVerano });
    }

    public async Task<ConfiguracionCalculo?> GetActivaBySectorAsync(int sectorId)
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo
                      WHERE sector_id = @SectorId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ConfiguracionCalculo>(query,
            new { SectorId = sectorId });
    }

    public async Task<IEnumerable<ConfiguracionCalculo>> GetBySectorAsync(int sectorId)
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo
                      WHERE sector_id = @SectorId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionCalculo>(query, new { SectorId = sectorId });
    }

    public async Task<ConfiguracionCalculo?> GetBySectorTipoTurnoAsync(int sectorId, string tipoTurno)
    {
        var query = @"SELECT id_configuracion as IdConfiguracion, sector_id as SectorId,
                             es_verano as EsVerano, horas_normales as HorasNormales,
                             horas_extras_oficiales as HorasExtrasOficiales,
                             horas_extras_adicionales as HorasExtrasAdicionales,
                             tolerancia_minutos as ToleranciaMinutos,
                             descuento_tarde_6_30_min as DescuentoTarde6a30Min,
                             descuento_tarde_31_mas as DescuentoTarde31Mas,
                             hora_entrada_esperada as HoraEntradaEsperada,
                             hora_salida_esperada as HoraSalidaEsperada,
                             tipo_turno as TipoTurno,
                             activo as Activo, fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_configuracion_calculo
                      WHERE sector_id = @SectorId AND tipo_turno = @TipoTurno AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ConfiguracionCalculo>(query,
            new { SectorId = sectorId, TipoTurno = tipoTurno });
    }

    public async Task<int> CreateAsync(ConfiguracionCalculo configuracion)
    {
        using var connection = _context.CreateConnection();

        // Si se marca como activo, desactivar las demás configuraciones según el tipo
        if (configuracion.Activo)
        {
            // Si tiene tipo_turno (sector rotativo), solo desactivar las del mismo tipo de turno
            // Si NO tiene tipo_turno (sector no rotativo), desactivar todas las del sector/temporada
            if (!string.IsNullOrEmpty(configuracion.TipoTurno))
            {
                var deactivateQuery = @"UPDATE ba_configuracion_calculo
                                       SET activo = 0
                                       WHERE sector_id = @SectorId AND es_verano = @EsVerano
                                       AND tipo_turno = @TipoTurno";
                await connection.ExecuteAsync(deactivateQuery,
                    new {
                        SectorId = configuracion.SectorId,
                        EsVerano = configuracion.EsVerano,
                        TipoTurno = configuracion.TipoTurno
                    });
            }
            else
            {
                var deactivateQuery = @"UPDATE ba_configuracion_calculo
                                       SET activo = 0
                                       WHERE sector_id = @SectorId AND es_verano = @EsVerano";
                await connection.ExecuteAsync(deactivateQuery,
                    new { SectorId = configuracion.SectorId, EsVerano = configuracion.EsVerano });
            }
        }

        var query = @"INSERT INTO ba_configuracion_calculo
                      (sector_id, es_verano, horas_normales, horas_extras_oficiales,
                       horas_extras_adicionales, tolerancia_minutos, descuento_tarde_6_30_min,
                       descuento_tarde_31_mas, hora_entrada_esperada, hora_salida_esperada,
                       tipo_turno, activo)
                      VALUES (@SectorId, @EsVerano, @HorasNormales, @HorasExtrasOficiales,
                              @HorasExtrasAdicionales, @ToleranciaMinutos, @DescuentoTarde6a30Min,
                              @DescuentoTarde31Mas, @HoraEntradaEsperada, @HoraSalidaEsperada,
                              @TipoTurno, @Activo);
                      SELECT CAST(SCOPE_IDENTITY() as int)";

        return await connection.QuerySingleAsync<int>(query, configuracion);
    }

    public async Task<bool> UpdateAsync(ConfiguracionCalculo configuracion)
    {
        using var connection = _context.CreateConnection();

        // Si se marca como activo, desactivar las demás configuraciones según el tipo
        if (configuracion.Activo)
        {
            // Si tiene tipo_turno (sector rotativo), solo desactivar las del mismo tipo de turno
            // Si NO tiene tipo_turno (sector no rotativo), desactivar todas las del sector/temporada
            if (!string.IsNullOrEmpty(configuracion.TipoTurno))
            {
                var deactivateQuery = @"UPDATE ba_configuracion_calculo
                                       SET activo = 0
                                       WHERE sector_id = @SectorId AND es_verano = @EsVerano
                                       AND tipo_turno = @TipoTurno AND id_configuracion != @IdConfiguracion";
                await connection.ExecuteAsync(deactivateQuery,
                    new {
                        SectorId = configuracion.SectorId,
                        EsVerano = configuracion.EsVerano,
                        TipoTurno = configuracion.TipoTurno,
                        IdConfiguracion = configuracion.IdConfiguracion
                    });
            }
            else
            {
                var deactivateQuery = @"UPDATE ba_configuracion_calculo
                                       SET activo = 0
                                       WHERE sector_id = @SectorId AND es_verano = @EsVerano
                                       AND id_configuracion != @IdConfiguracion";
                await connection.ExecuteAsync(deactivateQuery,
                    new {
                        SectorId = configuracion.SectorId,
                        EsVerano = configuracion.EsVerano,
                        IdConfiguracion = configuracion.IdConfiguracion
                    });
            }
        }

        var query = @"UPDATE ba_configuracion_calculo
                      SET sector_id = @SectorId, es_verano = @EsVerano,
                          horas_normales = @HorasNormales,
                          horas_extras_oficiales = @HorasExtrasOficiales,
                          horas_extras_adicionales = @HorasExtrasAdicionales,
                          tolerancia_minutos = @ToleranciaMinutos,
                          descuento_tarde_6_30_min = @DescuentoTarde6a30Min,
                          descuento_tarde_31_mas = @DescuentoTarde31Mas,
                          hora_entrada_esperada = @HoraEntradaEsperada,
                          hora_salida_esperada = @HoraSalidaEsperada,
                          tipo_turno = @TipoTurno,
                          activo = @Activo,
                          fecha_modificacion = GETDATE()
                      WHERE id_configuracion = @IdConfiguracion";

        var rowsAffected = await connection.ExecuteAsync(query, configuracion);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_configuracion_calculo WHERE id_configuracion = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }
}
