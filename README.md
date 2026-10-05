# MusicContest — Лабораторні роботи

Тема моделі: вокальний конкурс / телевізійний музичний проєкт.

## Структура

- `MusicContest.Common` — моделі, синхронний CRUD, **асинхронний CRUD**,
  демонстрації синхронізації (lock, SemaphoreSlim, AutoResetEvent),
  Parallel-створення, метод розширення.
- `MusicContest.Console` — демонстрація роботи ЛР1 (CRUD, статика,
  делегат, подія, серіалізація) та ЛР2 (async CRUD, thread safety,
  Parallel, lock, SemaphoreSlim, AutoResetEvent, LINQ).
- `MusicContest.Common.Tests` — xUnit-тести async CRUD та механізмів
  синхронізації.
- `docs/Lab1_Report.pdf` — PDF із результатом ЛР1.
- `docs/Lab2_Report.pdf` — PDF із результатом ЛР2.
- `docs/Lab2_build.txt`, `docs/Lab2_test.txt`, `docs/Lab2_output.txt`
  — реальні логи build/test/run.

## Класи моделі

- `ContestParticipant` — абстрактний базовий клас.
- `SoloSinger : ContestParticipant` — сольний виконавець.
- `VocalGroup : ContestParticipant` — вокальний гурт.
- `Song` — конкурсна пісня.
- `Performance` — конкурсний виступ.

У моделі використано конструктори, статичне поле, статичний конструктор,
звичайні й статичні методи, делегат, подію та метод розширення.

## CRUD (ЛР1)

`CrudService<T>` працює з будь-яким типом, який реалізує `IEntity`.
Дані зберігаються у `List<T>`. Реалізовано `Create`, `Read`, `ReadAll`,
`Update`, `Remove`. Додатково реалізовано `Save(string filePath)` та
`Load(string filePath)` із JSON-серіалізацією через `System.Text.Json`.

## ЛАБОРАТОРНА РОБОТА №2: Багатопотоковість. Асинхронність. IEnumerable. LINQ.

### Async CRUD

`CrudServiceAsync<T>` (`Services/CrudServiceAsync.cs`,
`Services/ICrudServiceAsync.cs`):

- асинхронні методи: `CreateAsync`, `ReadAsync`, `ReadAllAsync`,
  `ReadAllAsync(page, amount)`, `UpdateAsync`, `RemoveAsync`, `SaveAsync`;
- внутрішня колекція — `List<T>`, доступ захищений `lock` (критичні
  секції під `_gate`);
- методи читання та ітерація повертають **snapshot** (копію), а не
  внутрішню `List<T>`;
- `CreateAsync` при повторному Id — `InvalidOperationException`;
- `ReadAsync`/`UpdateAsync`/`RemoveAsync` при відсутньому Id —
  `KeyNotFoundException`;
- пагінація через LINQ: `Skip((page - 1) * amount).Take(amount)`,
  нумерація сторінок з 1; `page <= 0` або `amount <= 0` —
  `ArgumentOutOfRangeException`; сторінка поза межами — порожня
  колекція;
- `SaveAsync` — асинхронний JSON-запис через `File.WriteAllTextAsync`,
  захист від одночасного запису — `SemaphoreSlim` (лише один writer).

### Thread safety та синхронізація

- `lock` — демонстрація 8 потоків × 10 000 зростань (ожидане
  80 000, фактичне — з лічильника);
- `SemaphoreSlim` — 10 worker'ів, maxConcurrency = 3, фактичний
  максимум вимірюється атомарним лічильником;
- `AutoResetEvent` — справжня producer/consumer-демонстрація
  (M = 500), атомарні лічильники `producedCount`/`consumedCount`.

### IEnumerable<T>

`ICrudServiceAsync<T> : IEnumerable<T>`. `GetEnumerator` працює зі
snapshot, тому `foreach (var item in service)` безпечний серед
одноразових `CreateAsync`/`UpdateAsync`/`RemoveAsync`.

### LINQ

`Min`, `Max`, `Average`, `Where`, `Select`, `Aggregate`, `OrderBy` та
`Skip`/`Take` (пагінація) на числових властивості моделей (trivality
`DurationSeconds` пісень створених в Parallel).

### Parallel

`Parallell.cs` — паралельне створення 2000 об'єктів через
`Parallel.For` + `ConcurrentBag<Song>` + `Random.Shared`; фактичний
час вимірюється `Stopwatch`.

### Статичні фабрики

`SoloSinger.CreateNew()`, `VocalGroup.CreateNew()`, `Song.CreateNew()`,
`Performance.CreateNew()` — використовують `Random.Shared` (thread-safe).

### Тести

`MusicContest.Common.Tests` (xUnit):
- CreateAsync (dodatiy element, duplicate Id),
- ReadAsync (pravil'nenyi element, Unknown Id),
- Empty ReadAllAsync,
- UpdateAsync (update, unknown Id),
- RemoveAsync (remove, unknown Id),
- pagination, out-of-range, invalid page/amount,
- IEnumerable/foreach over the service,
- thread-safe parallel create (8 × 500 = 4000, Count == 4000),
- SaveAsync, concurrent SaveAsync (file valid, elements count).

### Запуск

```bash
dotnet restore
dotnet build
dotnet test MusicContest.Common.Tests/MusicContest.Common.Tests.csproj
dotnet run --project MusicContest.Console
```

Кожен запуск консольного застосунку — незалежна (детермінована)
сесія: перед демонстрацією `lab2_songs.json` очищається, тому
результати не накопичуються між запусками.

## Git / GitHub

Робоча гілка ЛР1: `lab1`.
Робоча гілка ЛР2 (Qwen 3.8, independent): `lab2-qwen38`.

Готова лабораторна робота №2 подається через Pull Request
`lab2-qwen38 -> lab1`.
