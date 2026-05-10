using Microsoft.AspNetCore.Http;
using OOP_lab1.Models;

namespace OOP_lab1.Http;

public sealed class RequestContext
{
    public RequestContext(HttpContext http, Dictionary<string, string> routeValues, SessionData? session)
    {
        Http = http;
        RouteValues = routeValues;
        Session = session;
    }

    public HttpContext Http { get; }
    public Dictionary<string, string> RouteValues { get; }
    public SessionData? Session { get; }
}

