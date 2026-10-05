# MusicContest — Лабораторні роботи №1–2

Тема моделі: вокальний конкурс / телевізійний музичний проєкт.

## Структура

- `MusicContest.Common` — моделі, CRUD-сервіс, метод розширення.
- `MusicContest.Console` — демонстрація роботи CRUD, статичних конструкцій, делегата, події та серіалізації.
- `docs/Lab1_Report.pdf` — PDF із результатом роботи консольного застосунку.

## Класи моделі

- `ContestParticipant` — абстрактний базовий клас.
- `SoloSinger : ContestParticipant` — сольний виконавець.
- `VocalGroup : ContestParticipant` — вокальний гурт.
- `Song` — конкурсна пісня.
- `Performance` — конкурсний виступ.

У моделі використано конструктори, статичне поле, статичний конструктор, звичайні й статичні методи, делегат, подію та метод розширення.

## CRUD

`CrudService<T>` працює з будь-яким типом, який реалізує `IEntity`. Дані зберігаються у `List<T>`. Реалізовано `Create`, `Read`, `ReadAll`, `Update`, `Remove`.

Додатково реалізовано `Save(string filePath)` та `Load(string filePath)` із JSON-серіалізацією через `System.Text.Json`.

## Запуск

```bash
dotnet restore
dotnet run --project MusicContest.Console
```

## Git / GitHub

Робоча гілка: `lab2` (для ЛР2).

Готова лабораторна робота подається через Pull Request `lab2 -> lab1`
(оскільки PR `lab1 -> master` на момент здачі ЛР2 ще відкритий).

## Laboratory Work #2 — Багатопотоковість. Асинхронність. IEnumerable. LINQ

Що реалізовано:

- Асинхронний generic CRUD-сервіс `ICrudServiceAsync<T>` : `IEnumerable<T>`
  (`CrudServiceAsync<T>`) — `CreateAsync`, `ReadAsync`,
  `ReadAllAsync`, `ReadAllAsync(page, amount)`, `UpdateAsync`,
  `RemoveAsync`, `SaveAsync`.
- Thread safety: `List<T>` захищений через `lock` (critical section),
  асинхронний запис файлу — через `SemaphoreSlim(1, 1)`.
- `IEnumerable<T>` реалізовано через `GetEnumerator()`,
  щоб `foreach` працював над snapshot'ом колекції.
- Пагінація через LINQ: `Skip((page - 1) * amount).Take(amount)`.
- Асинхронне збереження JSON (`System.Text.Json`, `File.WriteAllTextAsync`).
- Статичні фабрічні методи `CreateNew()` у `SoloSinger`, `VocalGroup`,
  `Song`, `Performance` з генеруванням даних через `Random.Shared`.
- Клас `Parallell` із `Parallel.For` для паралельного створення
  об'єктів (2000 об'єктів).
- LINQ статистика: `Count`, `Min`, `Max`, `Average`, `Where`, `Select`,
  `Aggregate`, `OrderBy`.
- Примітиви синхронізації: `lock`, `SemaphoreSlim`, `AutoResetEvent`
  (окремі демо-методи у `Parallell`).
- Модульні xUnit-тести для `ICrudServiceAsync<T>`
  (15 тестів, включаючи thread safety та concurrent save).

**Файли ЛР2:**

- `MusicContest.Common/Services/ICrudServiceAsync.cs` — інтерфейс.
- `MusicContest.Common/Services/CrudServiceAsync.cs` — реалізація.
- `MusicContest.Common/Parallell.cs` — паралельні приклади.
- `MusicContest.Common.Tests/` — xUnit-тести.
- `LAB2_CONTROL_QUESTIONS.md` — відповіді на 14 контрольних запитань.
- `MusicContest.Console/Program.cs` — демонстраційний запуск (блоки ЛР2).
- `docs/Lab2_output.txt` — реальний вихід консольного застосунку.
- `docs/Lab2_Report.pdf` — PDF-звіт з результатів виконання.

Запуск:
```bash
dotnet restore
dotnet build
dotnet test --project MusicContest.Common.Tests
dotnet run --project MusicContest.Console
```
