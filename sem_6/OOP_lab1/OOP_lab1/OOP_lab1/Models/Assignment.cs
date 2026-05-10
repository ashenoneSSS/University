namespace OOP_lab1.Models;

public enum AssignmentType
{
    Procedure,
    Medicine,
    Operation
}

public sealed record Assignment(
    int Id,
    int PatientId,
    AssignmentType Type,
    string Title,
    bool IsCompleted,
    int CreatedByUserId,
    int? CompletedByUserId,
    DateTimeOffset? CompletedAt
);

