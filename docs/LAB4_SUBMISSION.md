# Лабораторна робота №4. ASP.NET Core Web API

Проєкт: MusicContest.REST, .NET 8. Гілка: `lab4`.

## Архітектура

Контролери використовують DTO із `MusicContest.REST/Models` та інтерфейс `ICrudServiceAsync<T>`. Реалізація `RepositoryCrudServiceAsync<T>` звертається до `IRepository<T>`, `EfRepository<T>` і `MusicContestContext` із лабораторної №3. Залежності реєструються у `Program.cs`.

Ресурси: `/api/songs` і `/api/performances`. Кожен ресурс має GET (список і один запис), POST, PUT та DELETE. Список підтримує `page` і `amount`. Успішне створення повертає 201 із Location; читання повертає 200 або 404; успішні PUT/DELETE повертають 204; некоректні дані повертають 400.

## Запуск

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet run --project MusicContest.REST --urls http://127.0.0.1:5084
```

Swagger UI: http://127.0.0.1:5084/swagger

API використовує SQLite `docs/musiccontest.db` і застосовує EF Core міграції. Демонстраційні записи з лабораторної №3 можна створити командою `dotnet run --project MusicContest.Console -- --lab3-only`. Для перевірки створення пісні заповніть POST /api/songs, потім використайте отриманий `id` у GET, PUT і DELETE.

Звіт `docs/Lab4_Report.pdf` має формуватися тільки після фактичних HTTP-запитів. Якщо перевірка не запускалася, не слід зазначати її як виконану.

## Pull Request

Оскільки PR попередніх лабораторних відкриті, базовою гілкою для PR лабораторної №4 є `lab3`.
