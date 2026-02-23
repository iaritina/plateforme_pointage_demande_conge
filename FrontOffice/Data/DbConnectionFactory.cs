using System.Data;
using Microsoft.Data.SqlClient;

namespace FrontOffice.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' introuvable.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}