using Microsoft.Data.SqlClient;
using OOP_lab1.Models;

namespace OOP_lab1.Data;

public sealed class UserDao
{
    private readonly DbConnectionFactory _db;

    public UserDao(DbConnectionFactory db)
    {
        _db = db;
    }

    public (int id, string login, string password, string role)? FindByLogin(string login)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, login, password, role
            FROM dbo.users
            WHERE login = @login
            """;
        cmd.Parameters.Add(new SqlParameter("@login", System.Data.SqlDbType.NVarChar, 50) { Value = login });

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return (
            r.GetInt32(0),
            r.GetString(1),
            r.GetString(2),
            r.GetString(3)
        );
    }

    public User GetById(int id)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, login, role
            FROM dbo.users
            WHERE id = @id
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });

        using var r = cmd.ExecuteReader();
        if (!r.Read()) throw new InvalidOperationException("User not found");
        return new User(r.GetInt32(0), r.GetString(1), r.GetString(2));
    }

    public int Create(string login, string password, string role)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.users(login, password, role)
            OUTPUT INSERTED.id
            VALUES (@login, @password, @role)
            """;
        cmd.Parameters.Add(new SqlParameter("@login", System.Data.SqlDbType.NVarChar, 50) { Value = login });
        cmd.Parameters.Add(new SqlParameter("@password", System.Data.SqlDbType.NVarChar, 100) { Value = password });
        cmd.Parameters.Add(new SqlParameter("@role", System.Data.SqlDbType.NVarChar, 20) { Value = role });
        return (int)cmd.ExecuteScalar()!;
    }
}
