using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using OOP_lab1.Data;
using OOP_lab1.Http;
using OOP_lab1.Services;
using OOP_lab1.Sessions;

namespace OOP_lab1.Controllers;

public sealed class AuthController
{
    private readonly TemplateRenderer _views;
    private readonly AuthService _auth;
    private readonly SessionManager _sessions;
    private readonly UserDao _users;

    public AuthController(TemplateRenderer views, AuthService auth, SessionManager sessions, UserDao users)
    {
        _views = views;
        _auth = auth;
        _sessions = sessions;
        _users = users;
    }

    public async Task LoginGet(RequestContext ctx)
    {
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        var html = _views.Render("login.html", new Dictionary<string, string?>
        {
            ["error"] = ""
        });
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task LoginPost(RequestContext ctx)
    {
        var form = await ctx.Http.Request.ReadFormAsync();
        var login = (string?)form["login"];
        var password = (string?)form["password"];

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            await RenderError(ctx, "Enter login and password.");
            return;
        }

        try
        {
            var user = _auth.ValidateLogin(login, password);
            if (user is null)
            {
                await RenderError(ctx, "Invalid login or password.");
                return;
            }

            _sessions.Create(ctx.Http.Response, user.Id, user.Role);
            ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
            ctx.Http.Response.Headers.Location = "/patients";
        }
        catch (SqlException ex)
        {
            await RenderError(ctx, SqlErrorMessages.UserMessage(ex));
        }
    }

    public async Task RegisterGet(RequestContext ctx)
    {
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        var html = _views.Render("register.html", new Dictionary<string, string?>
        {
            ["error"] = ""
        });
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task RegisterPost(RequestContext ctx)
    {
        var form = await ctx.Http.Request.ReadFormAsync();
        var login = ((string?)form["login"])?.Trim();
        var password = (string?)form["password"];
        var role = ((string?)form["role"])?.Trim();

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
        {
            await RenderRegisterError(ctx, "Fill in all fields.");
            return;
        }

        if (!string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Nurse", StringComparison.OrdinalIgnoreCase))
        {
            await RenderRegisterError(ctx, "Invalid role.");
            return;
        }

        try
        {
            if (_users.FindByLogin(login) is not null)
            {
                await RenderRegisterError(ctx, "This login is already taken.");
                return;
            }

            _users.Create(login, password, role);
        }
        catch (SqlException ex) when (SqlErrorMessages.IsUniqueKeyViolation(ex))
        {
            await RenderRegisterError(ctx, "This login is already taken.");
            return;
        }
        catch (SqlException ex)
        {
            await RenderRegisterError(ctx, SqlErrorMessages.UserMessage(ex));
            return;
        }

        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/login";
    }

    public Task LogoutPost(RequestContext ctx)
    {
        _sessions.Destroy(ctx.Http.Request, ctx.Http.Response);
        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/login";
        return Task.CompletedTask;
    }

    private async Task RenderError(RequestContext ctx, string message)
    {
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        var html = _views.Render("login.html", new Dictionary<string, string?>
        {
            ["error"] = message
        });
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    private async Task RenderRegisterError(RequestContext ctx, string message)
    {
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        var html = _views.Render("register.html", new Dictionary<string, string?>
        {
            ["error"] = message
        });
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }
}

