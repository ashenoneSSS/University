using Microsoft.AspNetCore.Http;
using OOP_lab1.Sessions;

namespace OOP_lab1.Http;

public sealed class FrontController
{
    private readonly Router _router;
    private readonly SessionManager _sessions;

    public FrontController(Router router, SessionManager sessions)
    {
        _router = router;
        _sessions = sessions;
    }

    public async Task Handle(HttpContext http)
    {
        var path = http.Request.Path.Value ?? "/";
        var method = http.Request.Method;

        if (!IsPublic(path) && _sessions.Get(http.Request) is null)
        {
            http.Response.StatusCode = StatusCodes.Status302Found;
            http.Response.Headers.Location = "/login";
            return;
        }

        var match = _router.Match(method, path);
        if (match is null)
        {
            http.Response.StatusCode = StatusCodes.Status404NotFound;
            await http.Response.WriteAsync("Not found");
            return;
        }

        var session = _sessions.Get(http.Request);
        var ctx = new RequestContext(http, match.RouteValues, session);
        await match.Handler(ctx);
    }

    private static bool IsPublic(string path)
        => string.Equals(path, "/", StringComparison.OrdinalIgnoreCase)
           || string.Equals(path, "/login", StringComparison.OrdinalIgnoreCase)
           || string.Equals(path, "/register", StringComparison.OrdinalIgnoreCase);
}

