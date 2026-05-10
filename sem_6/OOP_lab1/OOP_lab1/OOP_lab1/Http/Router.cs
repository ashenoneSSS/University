namespace OOP_lab1.Http;

public sealed class Router
{
    private readonly List<Route> _routes = new();

    public void Add(string method, string pathTemplate, Func<RequestContext, Task> handler)
    {
        _routes.Add(new Route(method.ToUpperInvariant(), new PathTemplate(pathTemplate), handler));
    }

    public RouteMatch? Match(string method, string path)
    {
        method = method.ToUpperInvariant();
        foreach (var r in _routes)
        {
            if (r.Method != method) continue;
            if (r.Template.TryMatch(path, out var values))
                return new RouteMatch(values, r.Handler);
        }
        return null;
    }

    private sealed record Route(string Method, PathTemplate Template, Func<RequestContext, Task> Handler);
}

public sealed record RouteMatch(Dictionary<string, string> RouteValues, Func<RequestContext, Task> Handler);

internal sealed class PathTemplate
{
    private readonly Segment[] _segments;

    public PathTemplate(string template)
    {
        if (!template.StartsWith('/')) template = "/" + template;
        _segments = Split(template).Select(Parse).ToArray();
    }

    public bool TryMatch(string path, out Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(path)) path = "/";
        if (!path.StartsWith('/')) path = "/" + path;

        var parts = Split(path);
        if (parts.Length != _segments.Length)
        {
            values = new Dictionary<string, string>();
            return false;
        }

        values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < _segments.Length; i++)
        {
            var seg = _segments[i];
            var p = parts[i];

            if (seg.IsParam)
            {
                values[seg.Value] = Uri.UnescapeDataString(p);
                continue;
            }

            if (!string.Equals(seg.Value, p, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string[] Split(string path)
        => path.Trim('/').Length == 0 ? Array.Empty<string>() : path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static Segment Parse(string part)
    {
        if (part.StartsWith('{') && part.EndsWith('}') && part.Length > 2)
            return new Segment(true, part[1..^1]);
        return new Segment(false, part);
    }

    private sealed record Segment(bool IsParam, string Value);
}

