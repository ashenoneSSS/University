using OOP_lab1.Http;
using OOP_lab1.Models;
using OOP_lab1.Services;

namespace OOP_lab1.Tests;

public sealed class RouterTests
{
    [Fact]
    public void Router_resolves_method_and_path_params()
    {
        var router = new Router();
        Func<RequestContext, Task> h = _ => Task.CompletedTask;
        router.Add("GET", "/patients/{id}", h);

        var match = router.Match("GET", "/patients/123");

        Assert.NotNull(match);
        Assert.Equal("123", match!.RouteValues["id"]);
        Assert.Same(h, match.Handler);
    }
}

public sealed class AuthServiceTests
{
    [Fact]
    public void Auth_validates_login_and_password()
    {
        var ok = AuthService.IsValid((1, "doctor", "doctor", "Doctor"), "doctor", "doctor");
        var bad = AuthService.IsValid((1, "doctor", "doctor", "Doctor"), "doctor", "x");

        Assert.True(ok);
        Assert.False(bad);
    }
}

public sealed class RoleRulesTests
{
    [Fact]
    public void Nurse_cannot_execute_operation()
    {
        Assert.False(RoleRules.CanExecuteAssignment("Nurse", AssignmentType.Operation));
    }

    [Fact]
    public void Doctor_can_execute_any_assignment()
    {
        Assert.True(RoleRules.CanExecuteAssignment("Doctor", AssignmentType.Operation));
        Assert.True(RoleRules.CanExecuteAssignment("Doctor", AssignmentType.Procedure));
        Assert.True(RoleRules.CanExecuteAssignment("Doctor", AssignmentType.Medicine));
    }
}

public sealed class DischargeLogicTests
{
    [Fact]
    public void Patient_discharge_requires_final_diagnosis_and_no_open_assignments()
    {
        Assert.False(DischargeService.CanDischarge("", hasOpenAssignments: false));
        Assert.False(DischargeService.CanDischarge("ok", hasOpenAssignments: true));
        Assert.True(DischargeService.CanDischarge("ok", hasOpenAssignments: false));
        Assert.Equal("Final diagnosis is required.", DischargeService.ValidateDischarge("", hasOpenAssignments: false));
        Assert.Equal("Complete all assignments before discharge.", DischargeService.ValidateDischarge("ok", hasOpenAssignments: true));
        Assert.Null(DischargeService.ValidateDischarge("ok", hasOpenAssignments: false));
    }
}
