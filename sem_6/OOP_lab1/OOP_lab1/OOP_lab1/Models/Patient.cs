namespace OOP_lab1.Models;

public sealed record Patient(
    int Id,
    string FullName,
    DateOnly? BirthDate,
    string? Diagnosis,
    bool IsDischarged,
    string? FinalDiagnosis
);

