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

    public bool TryDischarge(int patientId, string finalDiagnosis)
    {
        var hasOpen = _assignments.AnyOpenByPatient(patientId);
        if (!CanDischarge(finalDiagnosis, hasOpen)) return false;
        _patients.Discharge(patientId, finalDiagnosis.Trim());
        return true;
    }

    public static bool CanDischarge(string finalDiagnosis, bool hasOpenAssignments)
        => !string.IsNullOrWhiteSpace(finalDiagnosis) && !hasOpenAssignments;
}

