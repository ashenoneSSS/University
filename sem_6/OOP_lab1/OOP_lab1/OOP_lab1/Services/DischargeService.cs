using OOP_lab1.Data;

namespace OOP_lab1.Services;

public sealed class DischargeService
{
    private readonly PatientDao _patients;
    private readonly AssignmentDao _assignments;

    public DischargeService(PatientDao patients, AssignmentDao assignments)
    {
        _patients = patients;
        _assignments = assignments;
    }

    public bool TryDischarge(int patientId, string finalDiagnosis, out string? error)
    {
        var hasOpen = _assignments.AnyOpenByPatient(patientId);
        error = ValidateDischarge(finalDiagnosis, hasOpen);
        if (error is not null) return false;

        _patients.Discharge(patientId, finalDiagnosis.Trim());
        return true;
    }

    public static bool CanDischarge(string finalDiagnosis, bool hasOpenAssignments)
        => ValidateDischarge(finalDiagnosis, hasOpenAssignments) is null;

    public static string? ValidateDischarge(string finalDiagnosis, bool hasOpenAssignments)
    {
        if (string.IsNullOrWhiteSpace(finalDiagnosis)) return "Final diagnosis is required.";
        if (hasOpenAssignments) return "Complete all assignments before discharge.";
        return null;
    }
}

