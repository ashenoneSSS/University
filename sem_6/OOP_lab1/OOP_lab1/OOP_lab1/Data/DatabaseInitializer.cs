using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OOP_lab1.Data;

public sealed class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = config.GetConnectionString("HospitalDb") ?? throw new InvalidOperationException("Missing connection string HospitalDb");
        _logger = logger;
    }

    public void EnsureCreated()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "HospitalLabDb" : builder.InitialCatalog;

        builder.InitialCatalog = "master";
        using (var master = new SqlConnection(builder.ConnectionString))
        {
            master.Open();
            Execute(master, $"""
                IF DB_ID(N'{EscapeSqlString(databaseName)}') IS NULL
                BEGIN
                    CREATE DATABASE {QuoteSqlName(databaseName)};
                END
                """);
        }

        builder.InitialCatalog = databaseName;
        using var database = new SqlConnection(builder.ConnectionString);
        database.Open();

        CreateTables(database);
        SeedData(database);
        _logger.LogInformation("Database {DatabaseName} is ready.", databaseName);
    }

    private static void CreateTables(SqlConnection conn)
    {
        Execute(conn, """
            IF OBJECT_ID(N'dbo.users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.users
                (
                    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    login NVARCHAR(50) NOT NULL UNIQUE,
                    password NVARCHAR(100) NOT NULL,
                    role NVARCHAR(20) NOT NULL
                );
            END
            """);

        Execute(conn, """
            IF OBJECT_ID(N'dbo.patients', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.patients
                (
                    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    full_name NVARCHAR(200) NOT NULL,
                    birth_date DATE NULL,
                    diagnosis NVARCHAR(400) NULL,
                    is_discharged BIT NOT NULL CONSTRAINT DF_patients_is_discharged DEFAULT(0),
                    final_diagnosis NVARCHAR(400) NULL
                );
            END
            """);

        Execute(conn, """
            IF OBJECT_ID(N'dbo.assignments', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.assignments
                (
                    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    patient_id INT NOT NULL,
                    type NVARCHAR(20) NOT NULL,
                    title NVARCHAR(200) NOT NULL,
                    is_completed BIT NOT NULL CONSTRAINT DF_assignments_is_completed DEFAULT(0),
                    created_by_user_id INT NOT NULL,
                    completed_by_user_id INT NULL,
                    completed_at DATETIME2 NULL,
                    CONSTRAINT FK_assignments_patients FOREIGN KEY (patient_id) REFERENCES dbo.patients(id) ON DELETE CASCADE,
                    CONSTRAINT FK_assignments_users_created FOREIGN KEY (created_by_user_id) REFERENCES dbo.users(id),
                    CONSTRAINT FK_assignments_users_completed FOREIGN KEY (completed_by_user_id) REFERENCES dbo.users(id)
                );
            END
            """);
    }

    private static void SeedData(SqlConnection conn)
    {
        Execute(conn, """
            IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE login = N'doctor')
            BEGIN
                INSERT INTO dbo.users(login, password, role)
                VALUES (N'doctor', N'doctor', N'Doctor');
            END

            IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE login = N'nurse')
            BEGIN
                INSERT INTO dbo.users(login, password, role)
                VALUES (N'nurse', N'nurse', N'Nurse');
            END
            """);

        Execute(conn, """
            IF NOT EXISTS (SELECT 1 FROM dbo.patients WHERE full_name = N'Ivanov Ivan')
            BEGIN
                INSERT INTO dbo.patients(full_name, birth_date, diagnosis, is_discharged, final_diagnosis)
                VALUES (N'Ivanov Ivan', '1990-01-10', N'Suspected bronchitis', 0, NULL);
            END

            IF NOT EXISTS (SELECT 1 FROM dbo.patients WHERE full_name = N'Petrov Petr')
            BEGIN
                INSERT INTO dbo.patients(full_name, birth_date, diagnosis, is_discharged, final_diagnosis)
                VALUES (N'Petrov Petr', '1985-05-02', N'Hypertension', 0, NULL);
            END
            """);

        Execute(conn, """
            DECLARE @doctorId INT = (SELECT TOP(1) id FROM dbo.users WHERE login = N'doctor');
            DECLARE @ivanovId INT = (SELECT TOP(1) id FROM dbo.patients WHERE full_name = N'Ivanov Ivan');
            DECLARE @petrovId INT = (SELECT TOP(1) id FROM dbo.patients WHERE full_name = N'Petrov Petr');

            IF @doctorId IS NOT NULL AND @ivanovId IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM dbo.assignments WHERE patient_id = @ivanovId AND title = N'Inhalation')
            BEGIN
                INSERT INTO dbo.assignments(patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at)
                VALUES (@ivanovId, N'procedure', N'Inhalation', 0, @doctorId, NULL, NULL);
            END

            IF @doctorId IS NOT NULL AND @ivanovId IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM dbo.assignments WHERE patient_id = @ivanovId AND title = N'Amoxicillin 500mg')
            BEGIN
                INSERT INTO dbo.assignments(patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at)
                VALUES (@ivanovId, N'medicine', N'Amoxicillin 500mg', 0, @doctorId, NULL, NULL);
            END

            IF @doctorId IS NOT NULL AND @petrovId IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM dbo.assignments WHERE patient_id = @petrovId AND title = N'Scheduled operation')
            BEGIN
                INSERT INTO dbo.assignments(patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at)
                VALUES (@petrovId, N'operation', N'Scheduled operation', 0, @doctorId, NULL, NULL);
            END
            """);
    }

    private static void Execute(SqlConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static string EscapeSqlString(string value)
        => value.Replace("'", "''", StringComparison.Ordinal);

    private static string QuoteSqlName(string value)
        => "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";
}
