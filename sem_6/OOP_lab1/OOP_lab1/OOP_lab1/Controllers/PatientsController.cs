using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using OOP_lab1.Data;
using OOP_lab1.Http;
using OOP_lab1.Services;

namespace OOP_lab1.Controllers;

public sealed class PatientsController
{
    private readonly PatientDao _patients;
    private readonly AssignmentDao _assignments;
    private readonly TemplateRenderer _views;
    private readonly DischargeService _discharge;

    public PatientsController(PatientDao patients, AssignmentDao assignments, TemplateRenderer views, DischargeService discharge)
    {
        _patients = patients;
        _assignments = assignments;
        _views = views;
        _discharge = discharge;
    }

    public async Task List(RequestContext ctx)
    {
        var list = _patients.List();
        var items = new StringBuilder();
        foreach (var p in list)
        {
            items.Append("<li>");
            items.Append("<a href=\"/patients/");
            items.Append(p.Id);
            items.Append("\">");
            items.Append(Html(p.FullName));
            items.Append("</a>");
            if (p.IsDischarged) items.Append(" (discharged)");
            items.Append("</li>");
        }

        var html = _views.Render("patients_list.html", new Dictionary<string, string?>
        {
            ["user_role"] = ctx.Session?.Role,
            ["create_link"] = RoleRules.IsDoctor(ctx.Session!.Role) ? "<a href=\"/patients/create\">Create patient</a>" : "",
            ["items"] = items.ToString()
        });
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task CreateGet(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        var html = _views.Render("patient_form.html", new Dictionary<string, string?>
        {
            ["title"] = "Create patient",
            ["action"] = "/patients/create",
            ["full_name"] = "",
            ["birth_date"] = "",
            ["diagnosis"] = "",
            ["submit"] = "Create"
        });
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task CreatePost(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        var form = await ctx.Http.Request.ReadFormAsync();
        var fullName = (string?)form["full_name"];
        var birthDateStr = (string?)form["birth_date"];
        var diagnosis = (string?)form["diagnosis"];

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var birthDate = ParseDate(birthDateStr);
        var id = _patients.Create(fullName.Trim(), birthDate, NullIfEmpty(diagnosis));

        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients/" + id;
    }

    public async Task Details(RequestContext ctx)
    {
        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var p = _patients.Get(id);
        if (p is null)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Http.Response.WriteAsync("Not found");
            return;
        }

        var assignments = _assignments.ListByPatient(id);
        var items = new StringBuilder();
        foreach (var a in assignments)
        {
            items.Append("<li>");
            items.Append(Html(a.Type.ToString()));
            items.Append(": ");
            items.Append(Html(a.Title));
            if (a.IsCompleted)
            {
                items.Append(" (completed)");
            }
            else
            {
                var canExecute = ctx.Session is not null && RoleRules.CanExecuteAssignment(ctx.Session.Role, a.Type);
                if (canExecute && !p.IsDischarged)
                {
                    items.Append(" <form style=\"display:inline\" method=\"post\" action=\"/assignments/");
                    items.Append(a.Id);
                    items.Append("/complete\"><button type=\"submit\">Complete</button></form>");
                }
            }
            items.Append("</li>");
        }

        var doctorActions = "";
        if (ctx.Session is not null && RoleRules.IsDoctor(ctx.Session.Role))
        {
            doctorActions =
                "<a href=\"/patients/" + id + "/edit\">Edit</a> " +
                "<form style=\"display:inline\" method=\"post\" action=\"/patients/" + id + "/delete\"><button type=\"submit\">Delete</button></form>";
        }

        var assignmentAdd = "";
        if (ctx.Session is not null && RoleRules.IsDoctor(ctx.Session.Role) && !p.IsDischarged)
        {
            assignmentAdd = _views.Render("assignment_add.html", new Dictionary<string, string?>
            {
                ["patient_id"] = id.ToString(CultureInfo.InvariantCulture)
            });
        }

        var dischargeForm = "";
        if (ctx.Session is not null && RoleRules.IsDoctor(ctx.Session.Role) && !p.IsDischarged)
        {
            dischargeForm = _views.Render("discharge_form.html", new Dictionary<string, string?>
            {
                ["patient_id"] = id.ToString(CultureInfo.InvariantCulture)
            });
        }

        var html = _views.Render("patient_details.html", new Dictionary<string, string?>
        {
            ["full_name"] = Html(p.FullName),
            ["birth_date"] = p.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
            ["diagnosis"] = Html(p.Diagnosis ?? ""),
            ["discharged"] = p.IsDischarged ? "Yes" : "No",
            ["final_diagnosis"] = Html(p.FinalDiagnosis ?? ""),
            ["doctor_actions"] = doctorActions,
            ["assignments"] = items.ToString(),
            ["assignment_add_form"] = assignmentAdd,
            ["discharge_form"] = dischargeForm
        });

        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task EditGet(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var p = _patients.Get(id);
        if (p is null)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Http.Response.WriteAsync("Not found");
            return;
        }

        var html = _views.Render("patient_form.html", new Dictionary<string, string?>
        {
            ["title"] = "Edit patient",
            ["action"] = "/patients/" + id + "/edit",
            ["full_name"] = HtmlAttr(p.FullName),
            ["birth_date"] = p.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
            ["diagnosis"] = HtmlAttr(p.Diagnosis ?? ""),
            ["submit"] = "Save"
        });
        ctx.Http.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Http.Response.WriteAsync(html, Encoding.UTF8);
    }

    public async Task EditPost(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var form = await ctx.Http.Request.ReadFormAsync();
        var fullName = (string?)form["full_name"];
        var birthDateStr = (string?)form["birth_date"];
        var diagnosis = (string?)form["diagnosis"];

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        _patients.Update(id, fullName.Trim(), ParseDate(birthDateStr), NullIfEmpty(diagnosis));
        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients/" + id;
    }

    public async Task DeletePost(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        _patients.Delete(id);
        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients";
    }

    public async Task DischargePost(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var form = await ctx.Http.Request.ReadFormAsync();
        var finalDiagnosis = (string?)form["final_diagnosis"];
        if (!_discharge.TryDischarge(id, finalDiagnosis ?? ""))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients/" + id;
    }

    private static DateOnly? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateOnly.TryParseExact(s.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
        return null;
    }

    private static string? NullIfEmpty(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Html(string s)
        => s.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static string HtmlAttr(string s)
        => Html(s).Replace("\"", "&quot;", StringComparison.Ordinal);
}

