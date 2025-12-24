using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class NovedadRepository : INovedadRepository
{
    private readonly DapperContext _context;

    public NovedadRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Novedad>> GetAllAsync()
    {
        var query = @"SELECT id_novedad as IdNovedad,
                             cod_novedad as CodNovedad,
                             desc_novedad as DescNovedad,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_novedades
                      ORDER BY cod_novedad";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Novedad>(query);
    }

    public async Task<Novedad?> GetByIdAsync(int id)
    {
        var query = @"SELECT id_novedad as IdNovedad,
                             cod_novedad as CodNovedad,
                             desc_novedad as DescNovedad,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_novedades
                      WHERE id_novedad = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Novedad>(query, new { Id = id });
    }

    public async Task<Novedad?> GetByCodNovedadAsync(string codNovedad)
    {
        var query = @"SELECT id_novedad as IdNovedad,
                             cod_novedad as CodNovedad,
                             desc_novedad as DescNovedad,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_novedades
                      WHERE cod_novedad = @CodNovedad";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Novedad>(query, new { CodNovedad = codNovedad });
    }

    public async Task<int> CreateAsync(Novedad novedad)
    {
        var query = @"INSERT INTO ba_novedades (cod_novedad, desc_novedad)
                      VALUES (@CodNovedad, @DescNovedad);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, novedad);
    }

    public async Task<bool> UpdateAsync(Novedad novedad)
    {
        var query = @"UPDATE ba_novedades
                      SET cod_novedad = @CodNovedad,
                          desc_novedad = @DescNovedad,
                          fecha_modificacion = GETDATE()
                      WHERE id_novedad = @IdNovedad";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, novedad);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_novedades WHERE id_novedad = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<ImportarNovedadesResult> ImportarDesdeTangoAsync()
    {
        var resultado = new ImportarNovedadesResult();

        try
        {
            // Leer novedades desde Tango
            var queryTango = $@"SELECT ID_NOVEDAD as IdNovedad,
                                      COD_NOVEDAD as CodNovedad,
                                      DESC_NOVEDAD as DescNovedad
                               FROM [{_context.TangoDbName}].[dbo].[NOVEDAD]";

            List<NovedadTango> novedadesTango;
            using (var tangoConn = _context.CreateConnection())
            {
                novedadesTango = (await tangoConn.QueryAsync<NovedadTango>(queryTango)).ToList();
            }

            if (!novedadesTango.Any())
            {
                resultado.Errores.Add("No se encontraron novedades en la base de datos de Tango");
                return resultado;
            }

            // Obtener novedades existentes en FichadasDB
            var novedadesExistentes = (await GetAllAsync()).ToList();

            using var connection = _context.CreateConnection();

            foreach (var novedadTango in novedadesTango)
            {
                try
                {
                    // Verificar si la novedad ya existe (por COD_NOVEDAD)
                    var novedadExistente = novedadesExistentes.FirstOrDefault(n => n.CodNovedad == novedadTango.CodNovedad);

                    if (novedadExistente != null)
                    {
                        // Actualizar si cambió la descripción
                        if (novedadExistente.DescNovedad != novedadTango.DescNovedad)
                        {
                            var updateQuery = @"UPDATE ba_novedades
                                              SET desc_novedad = @DescNovedad,
                                                  fecha_modificacion = GETDATE()
                                              WHERE cod_novedad = @CodNovedad";
                            await connection.ExecuteAsync(updateQuery, new
                            {
                                DescNovedad = novedadTango.DescNovedad,
                                CodNovedad = novedadTango.CodNovedad
                            });
                            resultado.NovedadesActualizadas++;
                        }
                        else
                        {
                            resultado.NovedadesExistentes++;
                        }
                    }
                    else
                    {
                        // Insertar nueva novedad
                        var insertQuery = @"INSERT INTO ba_novedades (cod_novedad, desc_novedad)
                                          VALUES (@CodNovedad, @DescNovedad)";
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            CodNovedad = novedadTango.CodNovedad,
                            DescNovedad = novedadTango.DescNovedad
                        });
                        resultado.NovedadesImportadas++;
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Error al importar novedad código {novedadTango.CodNovedad}: {ex.Message}");
                }
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Errores.Add($"Error general al importar novedades: {ex.Message}");
            return resultado;
        }
    }

    public async Task<IEnumerable<NovedadTango>> GetNovedadesDesdeTangoAsync()
    {
        try
        {
            // Leer novedades desde Tango
            var queryTango = $@"SELECT ID_NOVEDAD as IdNovedad,
                                      COD_NOVEDAD as CodNovedad,
                                      DESC_NOVEDAD as DescNovedad
                               FROM [{_context.TangoDbName}].[dbo].[NOVEDAD]
                               ORDER BY COD_NOVEDAD";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NovedadTango>(queryTango);
        }
        catch (Exception)
        {
            return Enumerable.Empty<NovedadTango>();
        }
    }

    public async Task<bool> ImportarNovedadEspecificaAsync(int idNovedadTango)
    {
        try
        {
            // Obtener la novedad de Tango
            var queryTango = $@"SELECT ID_NOVEDAD as IdNovedad,
                                      COD_NOVEDAD as CodNovedad,
                                      DESC_NOVEDAD as DescNovedad
                               FROM [{_context.TangoDbName}].[dbo].[NOVEDAD]
                               WHERE ID_NOVEDAD = @IdNovedad";

            NovedadTango? novedadTango;
            using (var tangoConn = _context.CreateConnection())
            {
                novedadTango = await tangoConn.QueryFirstOrDefaultAsync<NovedadTango>(queryTango, new { IdNovedad = idNovedadTango });
            }

            if (novedadTango == null)
            {
                return false; // No existe en Tango
            }

            // Verificar si ya existe en la base local (por código)
            var novedadExistente = await GetByCodNovedadAsync(novedadTango.CodNovedad);

            using var connection = _context.CreateConnection();

            if (novedadExistente != null)
            {
                // Actualizar si cambió la descripción
                if (novedadExistente.DescNovedad != novedadTango.DescNovedad)
                {
                    var updateQuery = @"UPDATE ba_novedades
                                      SET desc_novedad = @DescNovedad,
                                          fecha_modificacion = GETDATE()
                                      WHERE cod_novedad = @CodNovedad";
                    await connection.ExecuteAsync(updateQuery, new
                    {
                        DescNovedad = novedadTango.DescNovedad,
                        CodNovedad = novedadTango.CodNovedad
                    });
                    return true; // Actualizada
                }
                return false; // Ya existe y no cambió
            }

            // Insertar nueva novedad
            var insertQuery = @"INSERT INTO ba_novedades (cod_novedad, desc_novedad)
                              VALUES (@CodNovedad, @DescNovedad)";
            await connection.ExecuteAsync(insertQuery, new
            {
                CodNovedad = novedadTango.CodNovedad,
                DescNovedad = novedadTango.DescNovedad
            });

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
