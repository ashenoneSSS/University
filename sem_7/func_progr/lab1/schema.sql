SET NAMES utf8mb4;
DROP DATABASE IF EXISTS faculty_software;
CREATE DATABASE faculty_software CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE faculty_software;

CREATE TABLE categories (
    id   INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE authors (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    full_name    VARCHAR(150) NOT NULL,
    organization VARCHAR(150) NOT NULL,
    email        VARCHAR(150) NULL
);

CREATE TABLE software (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    name              VARCHAR(150) NOT NULL,
    annotation        TEXT NOT NULL,
    category_id       INT NOT NULL,
    version           VARCHAR(30) NOT NULL,
    distribution_path VARCHAR(255) NOT NULL,
    opened_at         DATE NOT NULL,
    valid_until       DATE NULL,
    usage_conditions  TEXT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

CREATE TABLE software_authors (
    software_id INT NOT NULL,
    author_id   INT NOT NULL,
    PRIMARY KEY (software_id, author_id),
    FOREIGN KEY (software_id) REFERENCES software(id) ON DELETE CASCADE,
    FOREIGN KEY (author_id) REFERENCES authors(id) ON DELETE CASCADE
);

CREATE TABLE users (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    role      VARCHAR(30) NOT NULL,
    unit      VARCHAR(100) NOT NULL,
    email     VARCHAR(150) NULL
);

CREATE TABLE usage_log (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    software_id INT NOT NULL,
    user_id     INT NOT NULL,
    used_on     DATE NOT NULL,
    minutes     INT NOT NULL,
    FOREIGN KEY (software_id) REFERENCES software(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE reviews (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    software_id INT NOT NULL,
    user_id     INT NOT NULL,
    rating      INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment     TEXT NOT NULL,
    created_on  DATE NOT NULL,
    FOREIGN KEY (software_id) REFERENCES software(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

INSERT INTO categories (name) VALUES
    ('Середовище розробки'), ('Система керування базами даних'), ('Математичний пакет'),
    ('Офісний пакет'), ('Система контролю версій'), ('Навчальна система');

INSERT INTO authors (full_name, organization, email) VALUES
    ('JetBrains s.r.o.', 'JetBrains', 'support@jetbrains.com'),
    ('Oracle Corporation', 'Oracle', NULL),
    ('Wolfram Research', 'Wolfram', NULL),
    ('The Document Foundation', 'TDF', NULL),
    ('Linus Torvalds', 'Git community', NULL),
    ('Коваленко Олег Петрович', 'Кафедра інформаційних систем', 'kovalenko@knu.ua'),
    ('Мельник Ірина Василівна', 'Кафедра інформаційних систем', 'melnyk@knu.ua');

INSERT INTO software (name, annotation, category_id, version, distribution_path, opened_at, valid_until, usage_conditions) VALUES
    ('IntelliJ IDEA', 'Інтегроване середовище розробки для Java та Kotlin', 1, '2026.2', '//fs-cyb/dist/ide/idea-2026.2.zip', '2025-09-01', '2026-08-31', 'Освітня ліцензія для студентів і викладачів'),
    ('MySQL Community Server', 'Реляційна система керування базами даних', 2, '8.4', '//fs-cyb/dist/db/mysql-8.4.msi', '2024-09-01', NULL, 'Вільна ліцензія GPL'),
    ('Mathematica', 'Пакет символьних і чисельних обчислень', 3, '14.1', '//fs-cyb/dist/math/mathematica-14.1.exe', '2025-02-01', '2027-01-31', 'Мережева ліцензія факультету, до 30 місць'),
    ('LibreOffice', 'Вільний офісний пакет', 4, '25.2', '//fs-cyb/dist/office/libreoffice-25.2.msi', '2024-09-01', NULL, 'Вільна ліцензія MPL'),
    ('Git', 'Розподілена система контролю версій', 5, '2.47', '//fs-cyb/dist/vcs/git-2.47.exe', '2024-09-01', NULL, 'Вільна ліцензія GPL'),
    ('Тренажер SQL', 'Навчальна система для практики SQL-запитів, розроблена на кафедрі', 6, '1.3', '//fs-cyb/dist/edu/sql-trainer-1.3.zip', '2025-10-01', '2026-06-30', 'Лише для студентів факультету');

INSERT INTO software_authors (software_id, author_id) VALUES
    (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (6, 7);

INSERT INTO users (full_name, role, unit, email) VALUES
    ('Савченко Володимир', 'студент', 'ІПС-42', NULL),
    ('Бондар Олена', 'студент', 'ІПС-41', NULL),
    ('Ткаченко Андрій', 'студент', 'ПІ-31', NULL),
    ('Мельник Ірина Василівна', 'викладач', 'Кафедра інформаційних систем', 'melnyk@knu.ua'),
    ('Шевчук Марія', 'співробітник', 'Обчислювальний центр', NULL);

INSERT INTO usage_log (software_id, user_id, used_on, minutes) VALUES
    (1, 1, '2026-10-01', 120), (1, 2, '2026-10-02', 95), (1, 3, '2026-10-02', 60),
    (2, 1, '2026-10-03', 80), (2, 4, '2026-10-05', 45), (3, 4, '2026-10-06', 150),
    (4, 5, '2026-10-06', 30), (5, 1, '2026-10-07', 25), (5, 2, '2026-10-07', 40),
    (6, 1, '2026-10-08', 70), (6, 2, '2026-10-09', 55), (6, 3, '2026-10-09', 65),
    (1, 1, '2026-10-10', 110), (2, 2, '2026-10-11', 50);

INSERT INTO reviews (software_id, user_id, rating, comment, created_on) VALUES
    (1, 1, 5, 'Зручне автодоповнення та рефакторинг', '2026-10-01'),
    (1, 2, 4, 'Потрібно багато пам''яті', '2026-10-02'),
    (2, 4, 5, 'Стабільна робота', '2026-10-05'),
    (3, 4, 4, 'Потужний, але складний для початківців', '2026-10-06'),
    (6, 1, 5, 'Корисні задачі до лабораторних', '2026-10-08'),
    (6, 3, 3, 'Хотілося б більше прикладів', '2026-10-09');
