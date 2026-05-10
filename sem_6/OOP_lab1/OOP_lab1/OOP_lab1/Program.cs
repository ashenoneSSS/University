using OOP_lab1.Data;
using OOP_lab1.Http;
using OOP_lab1.Sessions;
using OOP_lab1.Services;
using OOP_lab1.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<UserDao>();
builder.Services.AddSingleton<PatientDao>();
builder.Services.AddSingleton<AssignmentDao>();

builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<Router>();
builder.Services.AddSingleton<FrontController>();
builder.Services.AddSingleton(sp => new TemplateRenderer(builder.Environment.ContentRootPath));

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AuthController>();
builder.Services.AddSingleton<PatientsController>();
builder.Services.AddSingleton<AssignmentsController>();
builder.Services.AddSingleton<DischargeService>();

var app = builder.Build();

var router = app.Services.GetRequiredService<Router>();
router.Add("GET", "/", async ctx =>
{
    if (ctx.Session is not null)
    {
        ctx.Http.Response.StatusCode = StatusCodes.Status302Found;
        ctx.Http.Response.Headers.Location = "/patients";
        return;
    }

    var views = ctx.Http.RequestServices.GetRequiredService<TemplateRenderer>();
    var html = views.Render("index.html", new Dictionary<string, string?>());
    ctx.Http.Response.ContentType = "text/html; charset=utf-8";
    await ctx.Http.Response.WriteAsync(html);
});

var auth = app.Services.GetRequiredService<AuthController>();
router.Add("GET", "/login", auth.LoginGet);
router.Add("POST", "/login", auth.LoginPost);
router.Add("POST", "/logout", auth.LogoutPost);
router.Add("GET", "/register", auth.RegisterGet);
router.Add("POST", "/register", auth.RegisterPost);

var patients = app.Services.GetRequiredService<PatientsController>();
router.Add("GET", "/patients", patients.List);
router.Add("GET", "/patients/create", patients.CreateGet);
router.Add("POST", "/patients/create", patients.CreatePost);
router.Add("GET", "/patients/{id}", patients.Details);
router.Add("GET", "/patients/{id}/edit", patients.EditGet);
router.Add("POST", "/patients/{id}/edit", patients.EditPost);
router.Add("POST", "/patients/{id}/delete", patients.DeletePost);
router.Add("POST", "/patients/{id}/discharge", patients.DischargePost);

var assignments = app.Services.GetRequiredService<AssignmentsController>();
router.Add("POST", "/patients/{id}/assignments/add", assignments.AddToPatient);
router.Add("POST", "/assignments/{id}/complete", assignments.Complete);

app.Run(async http =>
{
    var fc = http.RequestServices.GetRequiredService<FrontController>();
    await fc.Handle(http);
});

app.Run();
