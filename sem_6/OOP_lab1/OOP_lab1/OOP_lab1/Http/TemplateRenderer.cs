using System.Text;

namespace OOP_lab1.Http;

public sealed class TemplateRenderer
{
    private readonly string _rootDir;

    public TemplateRenderer(string contentRootPath)
    {
        _rootDir = Path.Combine(contentRootPath, "Templates");
    }

    public string Render(string templateName, Dictionary<string, string?> values)
    {
        var path = Path.Combine(_rootDir, templateName);
        var html = File.ReadAllText(path, Encoding.UTF8);
        foreach (var kv in values)
        {
            html = html.Replace("{{" + kv.Key + "}}", kv.Value ?? "", StringComparison.Ordinal);
        }
        return html;
    }
}

