using Microsoft.AspNetCore.Http;
using OOP_lab1.Data;
using OOP_lab1.Http;
using OOP_lab1.Models;
using OOP_lab1.Services;

namespace OOP_lab1.Controllers;

public sealed class AssignmentsController
{
    private readonly AssignmentDao _assignments;
    private readonly PatientDao _patients;

    public AssignmentsController(AssignmentDao assignments, PatientDao patients)
    {
        _assignments = assignments;
        _patients = patients;
    }

    public async Task AddToPatient(RequestContext ctx)
    {
        if (!RoleRules.IsDoctor(ctx.Session!.Role))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        if (!ctx.RouteValues.TryGetValue("id", out var patientIdStr) || !int.TryParse(patientIdStr, out var patientId))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var p = _patients.Get(patientId);
        if (p is null)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Http.Response.WriteAsync("Not found");
            return;
        }

        if (p.IsDischarged)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var form = await ctx.Http.Request.ReadFormAsync();
        var typeStr = (string?)form["type"];
        var title = (string?)form["title"];
        if (string.IsNullOrWhiteSpace(typeStr) || string.IsNullOrWhiteSpace(title))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var type = ParseType(typeStr);
        _assignments.Create(patientId, type, title.Trim(), ctx.Session.UserId);

        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients/" + patientId;
    }

    public async Task Complete(RequestContext ctx)
    {
        if (!ctx.RouteValues.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        var a = _assignments.Get(id);
        if (a is null)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Http.Response.WriteAsync("Not found");
            return;
        }

        var p = _patients.Get(a.PatientId);
        if (p is null)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Http.Response.WriteAsync("Not found");
            return;
        }

        if (p.IsDischarged)
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Http.Response.WriteAsync("Bad request");
            return;
        }

        if (!RoleRules.CanExecuteAssignment(ctx.Session!.Role, a.Type))
        {
            ctx.Http.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Http.Response.WriteAsync("Forbidden");
            return;
        }

        _assignments.Complete(a.Id, ctx.Session.UserId);
        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients/" + a.PatientId;
    }

    private static AssignmentType ParseType(string s)
        => s.Trim().ToLowerInvariant() switch
        {
            "procedure" => AssignmentType.Procedure,
            "medicine" => AssignmentType.Medicine,
            "operation" => AssignmentType.Operation,
            _ => throw new InvalidOperationException("Unknown assignment type")
        };
}

