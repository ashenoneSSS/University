IF DB_ID(N'HospitalLabDb') IS NULL
BEGIN
    CREATE DATABASE HospitalLabDb;
END
GO

USE HospitalLabDb;
GO

IF OBJECT_ID(N'dbo.assignments', N'U') IS NOT NULL DROP TABLE dbo.assignments;
IF OBJECT_ID(N'dbo.patients', N'U') IS NOT NULL DROP TABLE dbo.patients;
IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL DROP TABLE dbo.users;
GO

CREATE TABLE dbo.users
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    login NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) NOT NULL
);

CREATE TABLE dbo.patients
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    full_name NVARCHAR(200) NOT NULL,
    birth_date DATE NULL,
    diagnosis NVARCHAR(400) NULL,
    is_discharged BIT NOT NULL CONSTRAINT DF_patients_is_discharged DEFAULT(0),
    final_diagnosis NVARCHAR(400) NULL
);

CREATE TABLE dbo.assignments
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    patient_id INT NOT NULL,
    type NVARCHAR(20) NOT NULL,
    title NVARCHAR(200) NOT NULL,
    is_completed BIT NOT NULL CONSTRAINT DF_assignments_is_completed DEFAULT(0),
    created_by_user_id INT NOT NULL,
    completed_by_user_id INT NULL,
    completed_at DATETIME2 NULL,
    CONSTRAINT FK_assignments_patients FOREIGN KEY (patient_id) REFERENCES dbo.patients(id) ON DELETE CASCADE,
    CONSTRAINT FK_assignments_users_created FOREIGN KEY (created_by_user_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_assignments_users_completed FOREIGN KEY (completed_by_user_id) REFERENCES dbo.users(id)
);

INSERT INTO dbo.users(login, password, role) VALUES
(N'doctor', N'doctor', N'Doctor'),
(N'nurse', N'nurse', N'Nurse');

INSERT INTO dbo.patients(full_name, birth_date, diagnosis, is_discharged, final_diagnosis) VALUES
(N'Ivanov Ivan', '1990-01-10', N'Suspected bronchitis', 0, NULL),
(N'Petrov Petr', '1985-05-02', N'Hypertension', 0, NULL);

INSERT INTO dbo.assignments(patient_id, type, title, is_completed, created_by_user_id, completed_by_user_id, completed_at) VALUES
(1, N'procedure', N'Inhalation', 0, 1, NULL, NULL),
(1, N'medicine', N'Amoxicillin 500mg', 0, 1, NULL, NULL),
(2, N'operation', N'Scheduled operation', 0, 1, NULL, NULL);
