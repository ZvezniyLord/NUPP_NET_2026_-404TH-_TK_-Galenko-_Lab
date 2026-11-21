# Подання лабораторної роботи №3

**Тема:** робота з базами даних у .NET. Використання Entity Framework Core.  
**Проєкт:** MusicContest, C# / .NET 8.  
**Студент:** Галенко. **Група:** 404TH. **Варіант:** TK.

## Матеріали

- [Гілка lab3](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab3)
- [Pull Request №4: lab3 → lab2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/4)
- [Короткий звіт із блок-схемою та посиланнями](PROJECT_REPORT.md)
- [Повний PDF-звіт](Lab3_Report.pdf)
- [ERD-діаграма](Lab3_ERD.png)
- [Файл бази даних SQLite](musiccontest.db)
- [Скріншоти структури та таблиць БД](Lab3_GUI_DatabaseStructure.png)
- [Скріншот таблиці учасників](Lab3_GUI_ContestParticipants.png)
- [Скріншот таблиці пісень](Lab3_GUI_Songs.png)
- [Скріншот таблиці виступів](Lab3_GUI_Performances.png)
- [Скріншот документів MongoDB](Lab3_MongoDB.png)
- [Контрольні запитання до ЛР3](../LAB3_CONTROL_QUESTIONS.md)

## Виконані завдання

1. Створено проєкт `MusicContest.Infrastructure` із моделями бази даних та контекстом `MusicContestContext`.
2. Налаштовано схему через Fluent API та реалізовано наслідування Table-per-Type (TPT).
3. Реалізовано зв'язки один-до-одного (1:1), один-до-багатьох (1:N) і багато-до-багатьох (M:N).
4. Створено й застосовано міграцію Entity Framework Core до SQLite.
5. Реалізовано `IRepository<T>`, `EfRepository<T>` та асинхронний CRUD-сервіс.
6. Оновлено консольний застосунок для роботи з реляційною базою даних.
7. Виконано додаткове завдання з MongoDB: документна модель і репозиторій для нереляційного сховища.

## Перевірка

Збірка, автоматизовані тести, консольна демонстрація SQLite та додатковий сценарій MongoDB виконуються через GitHub Actions. Фактичні журнали та скриншоти збережено в папці `docs`.

Команди для самостійної перевірки:

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet test MusicContest.sln
dotnet run --project MusicContest.Console -- --lab3-only
```

Для додаткової демонстрації MongoDB потрібен запущений сервер MongoDB:

```bash
dotnet run --project MusicContest.Console -- --lab3-nosql
```

Демонстраційні імена учасників і назви колективів є навчальними прикладами. Репертуар сформовано з українських пісень; тривалість і тональність використовуються як демонстраційні значення.
