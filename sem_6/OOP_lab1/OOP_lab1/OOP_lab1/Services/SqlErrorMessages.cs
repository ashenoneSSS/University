using Microsoft.Data.SqlClient;

namespace OOP_lab1.Services;

public static class SqlErrorMessages
{
    public static bool IsUniqueKeyViolation(SqlException ex)
        => ex.Number is 2601 or 2627;

    public static string UserMessage(SqlException ex)
        => ex.Number switch
        {
            4060 => "Cannot open database HospitalLabDb. Check that LocalDB is installed and the connection string points to the same SQL Server instance.",
            18456 => "SQL Server rejected the login. Check Windows authentication and that the server instance is correct.",
            _ => "A database error occurred. Check that SQL Server is running and the connection string in appsettings.json matches your setup."
        };
}
