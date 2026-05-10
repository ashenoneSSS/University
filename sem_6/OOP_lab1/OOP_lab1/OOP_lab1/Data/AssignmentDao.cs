using Microsoft.Data.SqlClient;
using OOP_lab1.Models;

namespace OOP_lab1.Data;

public sealed class AssignmentDao
{
    private readonly DbConnectionFactory _db;

    public AssignmentDao(DbConnectionFactory db)
    {
        _db = db;
    }

    public List<Assignment> ListByPatient(int patientId)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at
            FROM dbo.assignments
            WHERE patient_id = @patient_id
            ORDER BY id DESC
            """;
        cmd.Parameters.Add(new SqlParameter("@patient_id", System.Data.SqlDbType.Int) { Value = patientId });

        using var r = cmd.ExecuteReader();
        var list = new List<Assignment>();
        while (r.Read())
        {
            list.Add(new Assignment(
                r.GetInt32(0),
                r.GetInt32(1),
                ParseType(r.GetString(2)),
                r.GetString(3),
                r.GetBoolean(4),
                r.GetInt32(5),
                r.IsDBNull(6) ? null : r.GetInt32(6),
                r.IsDBNull(7) ? null : new DateTimeOffset(r.GetDateTime(7))
            ));
        }
        return list;
    }

    public Assignment? Get(int id)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at
            FROM dbo.assignments
            WHERE id = @id
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Assignment(
            r.GetInt32(0),
            r.GetInt32(1),
            ParseType(r.GetString(2)),
            r.GetString(3),
            r.GetBoolean(4),
            r.GetInt32(5),
            r.IsDBNull(6) ? null : r.GetInt32(6),
            r.IsDBNull(7) ? null : new DateTimeOffset(r.GetDateTime(7))
        );
    }

    public int Create(int patientId, AssignmentType type, string title, int createdByUserId)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.assignments(patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at)
            OUTPUT INSERTED.id
            VALUES (@patient_id, @type, @title, 0, @created_by_user_id, NULL, NULL)
            """;
        cmd.Parameters.Add(new SqlParameter("@patient_id", System.Data.SqlDbType.Int) { Value = patientId });
        cmd.Parameters.Add(new SqlParameter("@type", System.Data.SqlDbType.NVarChar, 20) { Value = ToDbType(type) });
        cmd.Parameters.Add(new SqlParameter("@title", System.Data.SqlDbType.NVarChar, 200) { Value = title });
        cmd.Parameters.Add(new SqlParameter("@created_by_user_id", System.Data.SqlDbType.Int) { Value = createdByUserId });
        return (int)cmd.ExecuteScalar()!;
    }

    public void Complete(int id, int completedByUserId)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.assignments
            SET is_completed = 1,
                completed_by_user_id = @completed_by_user_id,
                completed_at = SYSUTCDATETIME()
            WHERE id = @id AND is_completed = 0
            """;
        cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@completed_by_user_id", System.Data.SqlDbType.Int) { Value = completedByUserId });
        cmd.ExecuteNonQuery();
    }

    public bool AnyOpenByPatient(int patientId)
    {
        using var conn = _db.CreateOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT TOP(1) 1
            FROM dbo.assignments
            WHERE patient_id = @patient_id AND is_completed = 0
            """;
        cmd.Parameters.Add(new SqlParameter("@patient_id", System.Data.SqlDbType.Int) { Value = patientId });
        var result = cmd.ExecuteScalar();
        return result != null;
    }

    private static AssignmentType ParseType(string dbValue)
        => dbValue.ToLowerInvariant() switch
        {
            "procedure" => AssignmentType.Procedure,
            "medicine" => AssignmentType.Medicine,
            "operation" => AssignmentType.Operation,
            _ => throw new InvalidOperationException("Unknown assignment type")
        };

    private static string ToDbType(AssignmentType type)
        => type switch
        {
            AssignmentType.Procedure => "procedure",
            AssignmentType.Medicine => "medicine",
            AssignmentType.Operation => "operation",
            _ => throw new InvalidOperationException("Unknown assignment type")
        };
}

