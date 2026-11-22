# Лабораторна робота №4

Тема: розробка вебсерверів засобами ASP.NET Core.

Основний проєкт `MusicContest.REST` реалізує CRUD для пісень і виступів через асинхронний сервіс із лабораторної №3. Окремі DTO розташовані в `Models`, контролери в `Controllers`. Працює Swagger UI.

Додатковий проєкт `MusicContest.MVC` показує HTML-таблиці на основі наявного `MusicContestContext`.

## Запуск

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet run --project MusicContest.Console -- --lab3-only
dotnet run --project MusicContest.REST --urls http://127.0.0.1:5084
```

Swagger UI: http://127.0.0.1:5084/swagger

У другому терміналі:

```bash
python scripts/check_lab4_api.py
python scripts/generate_lab4_report.py
```

Додатковий MVC-застосунок:

```bash
dotnet run --project MusicContest.MVC --urls http://127.0.0.1:5085
```

Документація: [матеріали для подання](LAB4_SUBMISSION.md), [контрольні запитання](LAB4_CONTROL_QUESTIONS.md).

Звіт `Lab4_Report.pdf` створюється тільки після фактичного тестування HTTP API.
