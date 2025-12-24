using Microsoft.Data.SqlClient;
using System.Data;

namespace FichadasAPI.Data;

public class DapperContext
{
    private readonly IConfiguration _configuration;
    public string TangoDbName { get; }

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        TangoDbName = _configuration["TangoSettings:DatabaseName"] ?? throw new InvalidOperationException("TangoSettings:DatabaseName not configured");
    }

    public IDbConnection CreateConnection()
        => new SqlConnection(_configuration.GetConnectionString("FichadasDB"));
}
