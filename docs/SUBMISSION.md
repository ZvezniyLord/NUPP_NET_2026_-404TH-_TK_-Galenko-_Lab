# Подання лабораторної роботи №3

**Тема:** робота з базами даних у .NET. Використання Entity Framework Core.  
**Проєкт:** MusicContest, C# / .NET 8.  
**Студент:** Галенко. **Група:** 404TH. **Варіант:** TK.

## Матеріали

- [Гілка lab3](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab3)
- [Pull Request №4: lab3 → lab2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/4)
- [Короткий звіт із блок-схемою та ERD](PROJECT_REPORT.md)
- [Повний PDF-звіт](Lab3_Report.pdf)
- [ERD-діаграма](Lab3_ERD.png)
- [SQLite-база даних](musiccontest.db)
- [Скріншоти структури й таблиць БД](Lab3_GUI_DatabaseStructure.png)
- [Скріншот учасників](Lab3_GUI_ContestParticipants.png)
- [Скріншот пісень](Lab3_GUI_Songs.png)
- [Скріншот виступів](Lab3_GUI_Performances.png)
- [Скріншот документів MongoDB](Lab3_MongoDB.png)
- [Контрольні запитання](../LAB3_CONTROL_QUESTIONS.md)
- [Редакційна мовна політика](../EDITORIAL_LANGUAGE_POLICY.md)

## Виконані завдання

1. Створено `MusicContest.Infrastructure`, моделі бази даних і контекст `MusicContestContext`.
2. Налаштовано Fluent API та наслідування Table-per-Type (TPT).
3. Реалізовано зв'язки 1:1, 1:N і M:N.
4. Створено й застосовано міграцію EF Core до SQLite.
5. Реалізовано `IRepository<T>`, `EfRepository<T>) та асинхронний CRUD-сервіс.
6. Оновлено консольний застосунок для роботи з реляційною базою даних.
7. Виконано додаткове завдання з MongoDB та окремим репозиторієм.

## Перевірені результати

- Збірка .NET: успішна.
- Автоматизовані тести: 27 із 27 пройдено.
- SQLite: 6 учасників, 8 пісень, 6 профілів, 12 виступів.
- Зв'язки багато-до-багатьох: 12 записів у таблиці `ParticipantSongs`.
- Додаткове завдання: виконано CRUD-операції з документами MongoDB.
- До звіту додано ERD та скриншоти реальної бази даних і її таблиць.

## Самостійна перевірка

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet test MusicContest.sln
dotnet run --project MusicContest.Console -- --lab3-only
```

Для додаткової демонстрації потрібен запущений сервер MongoDB:

```bash
dotnet run --project MusicContest.Console -- --lab3-nosql
```

Оскільки Pull Request лабораторної роботи №2 ще відкритий, лабораторна робота №3 подається з гілки `lab3` до `lab2`.
