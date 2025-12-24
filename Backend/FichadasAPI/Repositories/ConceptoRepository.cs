using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class ConceptoRepository : IConceptoRepository
{
    private readonly DapperContext _context;

    public ConceptoRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Concepto>> GetAllAsync()
    {
        var query = @"SELECT id_concepto as IdConcepto,
                             id_concepto_tango as IdConceptoTango,
                             nro_concepto as NroConcepto,
                             desc_concepto as DescConcepto,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_conceptos
                      ORDER BY nro_concepto";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Concepto>(query);
    }

    public async Task<Concepto?> GetByIdAsync(int id)
    {
        var query = @"SELECT id_concepto as IdConcepto,
                             id_concepto_tango as IdConceptoTango,
                             nro_concepto as NroConcepto,
                             desc_concepto as DescConcepto,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_conceptos
                      WHERE id_concepto = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Concepto>(query, new { Id = id });
    }

    public async Task<Concepto?> GetByIdConceptoTangoAsync(int idConceptoTango)
    {
        var query = @"SELECT id_concepto as IdConcepto,
                             id_concepto_tango as IdConceptoTango,
                             nro_concepto as NroConcepto,
                             desc_concepto as DescConcepto,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_conceptos
                      WHERE id_concepto_tango = @IdConceptoTango";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Concepto>(query, new { IdConceptoTango = idConceptoTango });
    }

    public async Task<Concepto?> GetByNroConceptoAsync(int nroConcepto)
    {
        var query = @"SELECT id_concepto as IdConcepto,
                             id_concepto_tango as IdConceptoTango,
                             nro_concepto as NroConcepto,
                             desc_concepto as DescConcepto,
                             fecha_creacion as FechaCreacion,
                             fecha_modificacion as FechaModificacion
                      FROM ba_conceptos
                      WHERE nro_concepto = @NroConcepto";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Concepto>(query, new { NroConcepto = nroConcepto });
    }

    public async Task<int> CreateAsync(Concepto concepto)
    {
        var query = @"INSERT INTO ba_conceptos (id_concepto_tango, nro_concepto, desc_concepto)
                      VALUES (@IdConceptoTango, @NroConcepto, @DescConcepto);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, concepto);
    }

    public async Task<bool> UpdateAsync(Concepto concepto)
    {
        var query = @"UPDATE ba_conceptos
                      SET id_concepto_tango = @IdConceptoTango,
                          nro_concepto = @NroConcepto,
                          desc_concepto = @DescConcepto,
                          fecha_modificacion = GETDATE()
                      WHERE id_concepto = @IdConcepto";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, concepto);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_conceptos WHERE id_concepto = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<ImportarConceptosResult> ImportarDesdeTangoAsync()
    {
        var resultado = new ImportarConceptosResult();

        try
        {
            // Leer conceptos desde Tango
            var queryTango = $@"SELECT ID_CONCEPTO as IdConcepto,
                                      NRO_CONCEPTO as NroConcepto,
                                      DESC_CONCEPTO as DescConcepto
                               FROM [{_context.TangoDbName}].[dbo].[CONCEPTO]";

            List<ConceptoTango> conceptosTango;
            using (var tangoConnection = _context.CreateConnection())
            {
                conceptosTango = (await tangoConnection.QueryAsync<ConceptoTango>(queryTango)).ToList();
            }

            if (!conceptosTango.Any())
            {
                resultado.Errores.Add("No se encontraron conceptos en la base de datos de Tango");
                return resultado;
            }

            // Obtener conceptos existentes en FichadasDB
            var conceptosExistentes = (await GetAllAsync()).ToList();

            using var connection = _context.CreateConnection();

            foreach (var conceptoTango in conceptosTango)
            {
                try
                {
                    // Verificar si el concepto ya existe (por ID_CONCEPTO de Tango)
                    var conceptoExistente = conceptosExistentes.FirstOrDefault(c => c.IdConceptoTango == conceptoTango.IdConcepto);

                    if (conceptoExistente != null)
                    {
                        // Actualizar si cambió la descripción o número
                        if (conceptoExistente.DescConcepto != conceptoTango.DescConcepto ||
                            conceptoExistente.NroConcepto != conceptoTango.NroConcepto)
                        {
                            var updateQuery = @"UPDATE ba_conceptos
                                              SET nro_concepto = @NroConcepto,
                                                  desc_concepto = @DescConcepto,
                                                  fecha_modificacion = GETDATE()
                                              WHERE id_concepto_tango = @IdConceptoTango";
                            await connection.ExecuteAsync(updateQuery, new
                            {
                                NroConcepto = conceptoTango.NroConcepto,
                                DescConcepto = conceptoTango.DescConcepto,
                                IdConceptoTango = conceptoTango.IdConcepto
                            });
                            resultado.ConceptosActualizados++;
                        }
                        else
                        {
                            resultado.ConceptosExistentes++;
                        }
                    }
                    else
                    {
                        // Insertar nuevo concepto
                        var insertQuery = @"INSERT INTO ba_conceptos (id_concepto_tango, nro_concepto, desc_concepto)
                                          VALUES (@IdConceptoTango, @NroConcepto, @DescConcepto)";
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            IdConceptoTango = conceptoTango.IdConcepto,
                            NroConcepto = conceptoTango.NroConcepto,
                            DescConcepto = conceptoTango.DescConcepto
                        });
                        resultado.ConceptosImportados++;
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Error al importar concepto ID {conceptoTango.IdConcepto}: {ex.Message}");
                }
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Errores.Add($"Error general al importar conceptos: {ex.Message}");
            return resultado;
        }
    }

    public async Task<IEnumerable<ConceptoTango>> GetConceptosDesdeTangoAsync()
    {
        try
        {
            // Leer conceptos desde Tango
            var queryTango = $@"SELECT ID_CONCEPTO as IdConcepto,
                                      NRO_CONCEPTO as NroConcepto,
                                      DESC_CONCEPTO as DescConcepto
                               FROM [{_context.TangoDbName}].[dbo].[CONCEPTO]
                               ORDER BY NRO_CONCEPTO";

            using var tangoConnection = _context.CreateConnection();
            return await tangoConnection.QueryAsync<ConceptoTango>(queryTango);
        }
        catch (Exception)
        {
            return Enumerable.Empty<ConceptoTango>();
        }
    }

    public async Task<bool> ImportarConceptoEspecificoAsync(int idConceptoTango)
    {
        try
        {
            // Verificar si ya existe en la base local
            var conceptoExistente = await GetByIdConceptoTangoAsync(idConceptoTango);
            if (conceptoExistente != null)
            {
                return false; // Ya existe
            }

            // Obtener el concepto de Tango
            var queryTango = $@"SELECT ID_CONCEPTO as IdConcepto,
                                      NRO_CONCEPTO as NroConcepto,
                                      DESC_CONCEPTO as DescConcepto
                               FROM [{_context.TangoDbName}].[dbo].[CONCEPTO]
                               WHERE ID_CONCEPTO = @IdConcepto";

            ConceptoTango? conceptoTango;
            using (var tangoConnection = _context.CreateConnection())
            {
                conceptoTango = await tangoConnection.QueryFirstOrDefaultAsync<ConceptoTango>(queryTango, new { IdConcepto = idConceptoTango });
            }

            if (conceptoTango == null)
            {
                return false; // No existe en Tango
            }

            // Insertar el concepto en la base local
            var insertQuery = @"INSERT INTO ba_conceptos (id_concepto_tango, nro_concepto, desc_concepto)
                              VALUES (@IdConceptoTango, @NroConcepto, @DescConcepto)";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(insertQuery, new
            {
                IdConceptoTango = conceptoTango.IdConcepto,
                NroConcepto = conceptoTango.NroConcepto,
                DescConcepto = conceptoTango.DescConcepto
            });

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
