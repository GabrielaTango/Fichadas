using Dapper;
using FichadasAPI.Data;
using FichadasAPI.Models;

namespace FichadasAPI.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DapperContext _context;

    public UsuarioRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByUsuarioAsync(string usuario)
    {
        var query = "SELECT id_usuario as IdUsuario, usuario as UsuarioNombre, password as Password, mail as Mail, es_admin as EsAdmin FROM ba_usuarios WHERE usuario = @Usuario";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Usuario = usuario });
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        var query = "SELECT id_usuario as IdUsuario, usuario as UsuarioNombre, password as Password, mail as Mail, es_admin as EsAdmin FROM ba_usuarios WHERE id_usuario = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Id = id });
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        var query = "SELECT id_usuario as IdUsuario, usuario as UsuarioNombre, mail as Mail, es_admin as EsAdmin FROM ba_usuarios";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Usuario>(query);
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        var query = @"INSERT INTO ba_usuarios (usuario, password, mail, es_admin)
                      VALUES (@UsuarioNombre, @Password, @Mail, @EsAdmin);
                      SELECT CAST(SCOPE_IDENTITY() as int)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, usuario);
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        var query = @"UPDATE ba_usuarios
                      SET usuario = @UsuarioNombre, mail = @Mail, es_admin = @EsAdmin
                      WHERE id_usuario = @IdUsuario";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, usuario);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = "DELETE FROM ba_usuarios WHERE id_usuario = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int id, string hashedPassword)
    {
        var query = "UPDATE ba_usuarios SET password = @Password WHERE id_usuario = @Id";
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { Password = hashedPassword, Id = id });
        return rowsAffected > 0;
    }
}
