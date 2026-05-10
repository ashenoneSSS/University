using Microsoft.AspNetCore.Http;
using OOP_lab1.Models;

namespace OOP_lab1.Sessions;

public sealed class SessionManager
{
    private readonly object _lock = new();
    private readonly Dictionary<string, SessionData> _sessions = new();
    private const string CookieName = "sid";

    public SessionData? Get(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(CookieName, out var sid)) return null;
        if (string.IsNullOrWhiteSpace(sid)) return null;

        lock (_lock)
        {
            return _sessions.TryGetValue(sid, out var s) ? s : null;
        }
    }

    public SessionData Create(HttpResponse response, int userId, string role)
    {
        var sid = Guid.NewGuid().ToString("N");
        var session = new SessionData(sid, userId, role);

        lock (_lock)
        {
            _sessions[sid] = session;
        }

        response.Cookies.Append(
            CookieName,
            sid,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Path = "/"
            }
        );

        return session;
    }

    public void Destroy(HttpRequest request, HttpResponse response)
    {
        if (request.Cookies.TryGetValue(CookieName, out var sid) && !string.IsNullOrWhiteSpace(sid))
        {
            lock (_lock)
            {
                _sessions.Remove(sid);
            }
        }

        response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
    }
}

