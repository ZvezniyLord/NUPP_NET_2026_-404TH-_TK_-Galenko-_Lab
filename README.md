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
`Skip`/`Take` (пагінація) на числовій властивості моделей (тривалість
`DurationSeconds` пісень, створених у Parallel).

### Parallel

`Parallell.cs` — паралельне створення 2000 об'єктів через
`Parallel.For` + `ConcurrentBag<Song>` + `Random.Shared`; фактичний
час вимірюється `Stopwatch`.

### Статичні фабрики

`SoloSinger.CreateNew()`, `VocalGroup.CreateNew()`, `Song.CreateNew()`,
`Performance.CreateNew()` — використовують `Random.Shared` (thread-safe).

### Урок: як читати код ЛР2

Порядок читання: кожен пункт — один механізм і його файл.
Коментарі в коді перекладають англійські терміни українською —
те саме, що й у ЛР1.

1. **Асинхронність (async) та `Task<T>`** — файл `Services/ICrudServiceAsync.cs`.
   Асинхронний метод не повертає дані напряму, а повертає `Task<T>`
   — «обіцянку»: «дані з'являться потім». Викликаючий код зупиняється
   на `await` і чекає, не блокуючи потік пулу (інша робота на ньому
   може йти).
2. **Snapshot (миттєва копія)** — `Services/CrudServiceAsync.cs`,
   метод `BuildSnapshot()`. Повертати внутрішню `List<T>` назовні
   небезпечно: зовнішній код може її змінити, і сервіс не побачить.
   Тому під `lock` робиться `ToList()` — копія. Читання та `foreach`
   завжди бачять колекцію у стані «на момент фото», а не живий список.
3. **Критичні секції** — ті самі файли, методи `CreateAsync`,
   `UpdateAsync`, `RemoveAsync`. Перевірка «чи є такий Id?» і зміна
   (Add/заміна/RemoveAt) стоять в одному `lock`: перевірка та зміна —
   атомарна (atomic) дія, два потоки не можуть «вприснути» однаковий
   Id між ними.
4. **Пагінація через LINQ** — `ReadAllAsync(page, amount)`.
   Використовується вираз `Skip((page - 1) * amount).Take(amount)`:
   нумерація сторінок з 1; сторінка поза межами — порожня колекція
   (не помилка); недійсна `page`/`amount` —
   `ArgumentOutOfRangeException`.
5. **Збереження (SaveAsync)** — ті самі файли. Спочатку snapshot і
   JSON-серіалізація, потім `SemaphoreSlim` на рівні 1: одночасно в
   файл пише лише ОДИН запис (writer); `await WaitAsync` —
   асинхронне очікування (потік пулу не блокується). `finally`
   гарантує `Release` навіть, якщо запис упав.
6. **IEnumerable<T>** — інтерфейс `ICrudServiceAsync<T> : IEnumerable<T>`
   та метод `GetEnumerator()` у реалізації. Саме тому в `Program.cs`
   можна написати звичайний `foreach (var item in service)`:
   foreach іде по snapshot, а не по живому `List<T>`.
7. **Parallel** — `Services/Parallell.cs`. `Parallel.For(0, 2000, ...)`:
   тіло циклу виконують різні потоки пулу одночасно; збір —
   `ConcurrentBag<Song>` (thread-safe, без lock); час вимірює
   `Stopwatch`.
8. **Lock-демонстрація (гоніжка потоків)** — `Services/SynchronizationDemos.cs`.
   Операція `counter++` — це три кроки: read-modify-write (прочитати,
   змінити, записати). Без синхронізації два потоки читають одне й те
   саме значення — одне зростання губиться. `lock (gate)` — взаємне
   виключення (mutual exclusion): в критичній секції одночасно один
   потік, інші чекають.
9. **SemaphoreSlim-демонстрація** — ті самі файли. У семафора 3
   «ліцензії»: `WaitAsync` бере одну, `Release` повертає — в ділянці
   одночасно не більше 3 worker'ів. Важливо: максимум паралельності
   НЕ написано вручну, а ВИМІРЯНО атомарним лічильником
   (`Interlocked.Increment` + `Interlocked.CompareExchange`).
10. **AutoResetEvent producer/consumer** — ті самі файли.
    Producer кладє елементи у буфер `ConcurrentQueue<int>` і на кожен
    з них робить `itemReady.Set()` — пробуджує рівно ОДНОГО потік,
    який чекає `WaitOne()`. Consumer справді знімає елементи
    (`TryDequeue`) і їх рахує. Deadlock (мертве блокування)
    неможливий: producer ні на що не чекає.
11. **Атомарні операції** — `Interlocked.Increment`/`Decrement` та
    `Volatile.Read` (читання актуального значення, без старих
    копій із кешу CPU). Усі демо повертають СПРАВЖНІ (real) значення
    з лічильників, а не числа з коментаря — саме їх перевірять тести.
12. **Тести** — `MusicContest.Common.Tests`: кожен механізм
    перевіряється окремим тестом (lock: actual == 8 × 10 000;
    semaphore: виміряний max ≤ 3, якби семафора не було — був би 10;
    producer/consumer: produced == consumed == 500 і порожній буфер).

### Тести

`MusicContest.Common.Tests` (xUnit):
- CreateAsync: додає елемент, duplicate Id → InvalidOperationException;
- ReadAsync: правильний елемент, невідомий Id → KeyNotFoundException;
- ReadAllAsync: порожній сервіс → порожня колекція;
- UpdateAsync: оновлення, невідомий Id → KeyNotFoundException;
- RemoveAsync: видалення, невідомий Id → KeyNotFoundException;
- пагінація: правильна сторінка, out-of-range, invalid page/amount;
- IEnumerable/foreach по сервісу (snapshot);
- thread-safe parallel create: 8 × 500 = 4 000, Count == 4 000;
- SaveAsync, concurrent SaveAsync: файл валідний, елементів рівно стільки.

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
