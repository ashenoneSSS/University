using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OOP_lab1.Data;

public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("HospitalDb") ?? throw new InvalidOperationException("Missing connection string HospitalDb");
    }

    public SqlConnection CreateOpen()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }
}
