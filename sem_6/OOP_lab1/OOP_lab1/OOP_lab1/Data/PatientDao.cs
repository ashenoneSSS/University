using Microsoft.Data.SqlClient;
using OOP_lab1.Models;

namespace OOP_lab1.Data;

public sealed class PatientDao
{
    private readonly DbConnectionFactory _db;

    public PatientDao(DbConnectionFactory db)
    {
        _db = db;
    }

    public List<Patient> List()
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, full_name, birth_date, diagnosis, is_discharged, final_diagnosis
            FROM dbo.patients
            ORDER BY id DESC
            """;

        using var r = cmd.ExecuteReader();
        var list = new List<Patient>();
        while (r.Read())
        {
            list.Add(new Patient(
                r.GetInt32(0),
                r.GetString(1),
                r.IsDBNull(2) ? null : DateOnly.FromDateTime(r.GetDateTime(2)),
                r.IsDBNull(3) ? null : r.GetString(3),
                r.GetBoolean(4),
                r.IsDBNull(5) ? null : r.GetString(5)
            ));
        }
        return list;
    }

    public Patient? Get(int id)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, full_name, birth_date, diagnosis, is_discharged, final_diagnosis
            FROM dbo.patients
            WHERE id = @id
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Patient(
            r.GetInt32(0),
            r.GetString(1),
            r.IsDBNull(2) ? null : DateOnly.FromDateTime(r.GetDateTime(2)),
            r.IsDBNull(3) ? null : r.GetString(3),
            r.GetBoolean(4),
            r.IsDBNull(5) ? null : r.GetString(5)
        );
    }

    public int Create(string fullName, DateOnly? birthDate, string? diagnosis)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.patients(full_name, birth_date, diagnosis, is_discharged, final_diagnosis)
            OUTPUT INSERTED.id
            VALUES (@full_name, @birth_date, @diagnosis, 0, NULL)
            """;
        cmd.Parameters.Add(new SqlParameter("@full_name", System.Data.SqlDbType.NVarChar, 200) { Value = fullName });
        cmd.Parameters.Add(new SqlParameter("@birth_date", System.Data.SqlDbType.Date) { Value = birthDate.HasValue ? birthDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@diagnosis", System.Data.SqlDbType.NVarChar, 400) { Value = diagnosis ?? (object)DBNull.Value });

        return (int)cmd.ExecuteScalar()!;
    }

    public void Update(int id, string fullName, DateOnly? birthDate, string? diagnosis)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.patients
            SET full_name = @full_name,
                birth_date = @birth_date,
                diagnosis = @diagnosis
            WHERE id = @id
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@full_name", System.Data.SqlDbType.NVarChar, 200) { Value = fullName });
        cmd.Parameters.Add(new SqlParameter("@birth_date", System.Data.SqlDbType.Date) { Value = birthDate.HasValue ? birthDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@diagnosis", System.Data.SqlDbType.NVarChar, 400) { Value = diagnosis ?? (object)DBNull.Value });
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.patients WHERE id = @id";
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });
        cmd.ExecuteNonQuery();
    }

    public void Discharge(int id, string finalDiagnosis)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.patients
            SET is_discharged = 1,
                final_diagnosis = @final_diagnosis
            WHERE id = @id AND is_discharged = 0
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@final_diagnosis", System.Data.SqlDbType.NVarChar, 400) { Value = finalDiagnosis });
        cmd.ExecuteNonQuery();
    }
}

