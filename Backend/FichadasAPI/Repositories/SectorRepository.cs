using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class SectorRepository : ISectorRepository
{
    private readonly DapperContext _context;

    public SectorRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sector>> GetAllAsync()
    {
        var query = @"SELECT s.id_sector as IdSector, s.nombre as Nombre, s.es_rotativo as EsRotativo,
                             s.novedad_extras_id as NovedadExtrasId,
                             s.novedad_trabajadas_id as NovedadTrabajadasId,
                             nExtras.cod_novedad as NovedadExtrasCodigo,
                             nExtras.desc_novedad as NovedadExtrasDescripcion,
                             nTrabajadas.cod_novedad as NovedadTrabajadasCodigo,
                             nTrabajadas.desc_novedad as NovedadTrabajadasDescripcion
                      FROM ba_sectores s
                      LEFT JOIN ba_novedades nExtras ON s.novedad_extras_id = nExtras.id_novedad
                      LEFT JOIN ba_novedades nTrabajadas ON s.novedad_trabajadas_id = nTrabajadas.id_novedad";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Sector>(query);
    }

    public async Task<Sector?> GetByIdAsync(int id)
    {
        var query = @"SELECT s.id_sector as IdSector, s.nombre as Nombre, s.es_rotativo as EsRotativo,
                             s.novedad_extras_id as NovedadExtrasId,
                             s.novedad_trabajadas_id as NovedadTrabajadasId,
                             nExtras.cod_novedad as NovedadExtrasCodigo,
                             nExtras.desc_novedad as NovedadExtrasDescripcion,
                             nTrabajadas.cod_novedad as NovedadTrabajadasCodigo,
                             nTrabajadas.desc_novedad as NovedadTrabajadasDescripcion
                      FROM ba_sectores s
                      LEFT JOIN ba_novedades nExtras ON s.novedad_extras_id = nExtras.id_novedad
                      LEFT JOIN ba_novedades nTrabajadas ON s.novedad_trabajadas_id = nTrabajadas.id_novedad
                      WHERE s.id_sector = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Sector>(query, new { Id = id });
    }

    public async Task<int> CreateAsync(Sector sector)
    {
        var query = @"INSERT INTO ba_sectores (nombre, es_rotativo, novedad_extras_id, novedad_trabajadas_id)
                      VALUES (@Nombre, @EsRotativo, @NovedadExtrasId, @NovedadTrabajadasId);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, sector);
    }

    public async Task<bool> UpdateAsync(Sector sector)
    {
        var query = @"UPDATE ba_sectores
                      SET nombre = @Nombre, es_rotativo = @EsRotativo, novedad_extras_id = @NovedadExtrasId, novedad_trabajadas_id = @NovedadTrabajadasId
                      WHERE id_sector = @IdSector";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, sector);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_sectores WHERE id_sector = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }
}
