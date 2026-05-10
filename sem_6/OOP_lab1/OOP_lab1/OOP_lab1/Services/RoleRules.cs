using OOP_lab1.Models;

namespace OOP_lab1.Services;

public static class RoleRules
{
    public static bool CanExecuteAssignment(string role, AssignmentType type)
    {
        if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.Equals(role, "Nurse", StringComparison.OrdinalIgnoreCase)) return false;
        return type is AssignmentType.Procedure or AssignmentType.Medicine;
    }

    public static bool IsDoctor(string role)
        => string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase);
}

