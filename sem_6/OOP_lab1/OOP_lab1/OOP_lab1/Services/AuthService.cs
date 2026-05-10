using OOP_lab1.Data;
using OOP_lab1.Models;

namespace OOP_lab1.Services;

public sealed class AuthService
{
    private readonly UserDao _users;

    public AuthService(UserDao users)
    {
        _users = users;
    }

    public User? ValidateLogin(string login, string password)
    {
        var row = _users.FindByLogin(login);
        if (!IsValid(row, login, password)) return null;
        return new User(row!.Value.id, row.Value.login, row.Value.role);
    }

    public static bool IsValid((int id, string login, string password, string role)? row, string login, string password)
    {
        if (row is null) return false;
        if (!string.Equals(row.Value.login, login, StringComparison.Ordinal)) return false;
        return string.Equals(row.Value.password, password, StringComparison.Ordinal);
    }
}

