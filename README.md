# MusicContest — Лабораторні роботи

> Мова матеріалів: українська; англійська використовується для технічних назв і міжнародних матеріалів. [Мовна політика](EDITORIAL_LANGUAGE_POLICY.md) · [Редакційний стандарт](EDITORIAL_WRITING_STANDARD.md).

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
  містять журнали build/test/run.

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

- асинхронні методи за інтерфейсом завдання: `CreateAsync`/`UpdateAsync`/
  `RemoveAsync` повертають `Task<bool>`, `ReadAsync` — `Task<T>`,
  `ReadAllAsync` та `ReadAllAsync(page, amount)` — `Task<IEnumerable<T>>`,
  `SaveAsync()` — `Task<bool>`;
- внутрішня колекція — `ConcurrentDictionary<Guid, T>` є вбудованою потокобезпечною колекцією .NET (`System.Collections.Concurrent`). Її операції
  `TryAdd`/`TryUpdate`/`TryRemove` атомарні під конкурентним доступом;
- методи читання та ітерація повертають **snapshot** (копію послідовності,
  впорядковану за Id), а не саму внутрішню колекцію. Це забезпечує
  стабільний порядок елементів під час читання; копія не є глибокою копією
  самих об'єктів.
- `CreateAsync` при повторному Id — `false`; `UpdateAsync`/`RemoveAsync`
  при відсутньому Id — `false`; `ReadAsync` при відсутньому Id —
  `KeyNotFoundException`;
- пагінація через LINQ: `Skip((page - 1) * amount).Take(amount)`,
  нумерація сторінок з 1; `page <= 0` або `amount <= 0` —
  `ArgumentOutOfRangeException`; сторінка поза межами — порожня
  колекція;
- сервіс зберігає колекцію у файл за властивістю `FilePath`:
  `SaveAsync()` записує JSON через `File.WriteAllTextAsync`.
  `SemaphoreSlim` не допускає одночасного запису кількох операцій.

### Thread safety та синхронізація

- `lock` — демонстрація 8 потоків × 10 000 зростань (очікуване значення — 80 000; фактичне зчитується з лічильника);
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

`Parallell.cs` демонструє створення 2000 об'єктів, що перевищує мінімальну
вимогу в 1000 об'єктів. `Parallel.For` створює пісні через `Song.CreateNew()`
та запускає їх додавання до `CrudServiceAsync<Song>`. Після завершення циклу
`Task.WhenAll` очікує всі запущені операції. Час вимірює `Stopwatch`, а
кількість елементів перевіряється за даними самого сервісу.

### Статичні фабрики

`SoloSinger.CreateNew()`, `VocalGroup.CreateNew()`, `Song.CreateNew()`,
`Performance.CreateNew()` використовують `Random.Shared`, який безпечний для паралельного доступу.

### Урок: як читати код ЛР2

Порядок читання побудовано за механізмами. Біля кожного пункту вказано відповідний файл.
Коментарі до коду пояснюють призначення механізмів і перекладають основні англомовні терміни.

1. **Асинхронність (async) та `Task<T>`.** Контракт описано у файлі
   `Services/ICrudServiceAsync.cs`. `Task<T>` представляє операцію, яка
   завершиться пізніше та поверне значення типу `T`. Оператор `await`
   призупиняє поточний асинхронний метод, не блокуючи потік на час
   асинхронного очікування.
2. **Snapshot (знімок послідовності).** Метод `BuildSnapshot()` у
   `Services/CrudServiceAsync.cs` формує список із `_items.Values`,
   упорядкований за `Id`. Так зовнішній код не отримує сам словник і
   порядок читання залишається передбачуваним. Це поверхнева копія:
   для посилальних типів елементи всередині списку залишаються тими самими
   об'єктами, тому зміна їхніх властивостей може бути видимою сервісу.
3. **Потокобезпечна колекція.** У `CreateAsync`, `UpdateAsync` і
   `RemoveAsync` використано `ConcurrentDictionary<Guid, T>`.
   `TryAdd` не допускає повторного ключа, `TryUpdate` змінює наявний запис,
   а `TryRemove` видаляє його атомарно. Для цих операцій окремий `lock`
   не потрібен, оскільки синхронізацію забезпечує сама колекція.
4. **Пагінація через LINQ.** Її реалізовано в `ReadAllAsync(page, amount)`.
   Використовується вираз `Skip((page - 1) * amount).Take(amount)`:
   нумерація сторінок з 1; сторінка поза межами — порожня колекція
   (не помилка); недійсна `page`/`amount` —
   `ArgumentOutOfRangeException`.
5. **Збереження (SaveAsync).** Метод створює знімок колекції та серіалізує
   його у JSON. Перед записом він очікує дозвіл через `SemaphoreSlim`.
   Блок `finally` повертає дозвіл методом `Release()`, навіть якщо запис
   завершується помилкою.
6. **IEnumerable<T>.** Інтерфейс `ICrudServiceAsync<T> : IEnumerable<T>`
   та метод `GetEnumerator()` у реалізації. Саме тому в `Program.cs`
   можна написати звичайний `foreach (var item in service)`:
   foreach іде по snapshot, а не по живій колекції сервісу.
7. **Parallel.** Реалізація міститься у `Services/Parallell.cs`. `Parallel.For(0, count, ...)`: тіло паралельно створює об'єкти та запускає їх додавання у CRUD-сервіс:
   кожний `Song.CreateNew()` + запуск `CreateAsync` виконується різними потоками пулу без блокування; після завершення `Parallel.For` метод очікує всі завдання через `Task.WhenAll`;
   `ConcurrentDictionary` у сервісі thread-safe сам; реальну кількість зчитують з сервісу, час вимірює
   `Stopwatch`.
8. **Демонстрація `lock` і стану гонитви потоків.** Код міститься у `Services/SynchronizationDemos.cs`.
   Операція `counter++` складається з трьох кроків: читання, зміни та запису.
   Без синхронізації два потоки можуть прочитати однакове значення, тому
   одне з оновлень буде втрачено. `lock (gate)` забезпечує взаємне
   виключення. У критичній секції працює один потік, решта очікує.
9. **Демонстрація SemaphoreSlim.** Семафор має три дозволи.
   `WaitAsync` отримує один із них, а `Release` повертає його.
   Тому в захищеній ділянці одночасно працює не більше трьох завдань.
   Фактичний максимум вимірюється атомарним лічильником за допомогою
   `Interlocked.Increment` і `Interlocked.CompareExchange`.
10. **AutoResetEvent у схемі producer/consumer.** Producer кладе
    елементи в `ConcurrentQueue<int>` і викликає `itemReady.Set()`.
    Сигнал пробуджує одного потоку, який очікує через `WaitOne()`.
    Consumer забирає елементи методом `TryDequeue` та підраховує їх.
    Producer не очікує звільнення буфера, тому в цій демонстрації
    немає циклічного очікування між виробником і споживачем.
11. **Атомарні операції.** `Interlocked.Increment` і `Interlocked.Decrement`
    змінюють лічильники атомарно. `Volatile.Read` забезпечує відповідні
    гарантії під час читання значення. Демонстрації повертають фактичні
    результати, отримані під час виконання; тести перевіряють саме їх.
12. **Тести.** У `MusicContest.Common.Tests` кожен механізм
    перевіряється окремим тестом (lock: actual == 8 × 10 000;
    semaphore: виміряний max ≤ 3, якби семафора не було — був би 10;
    producer/consumer: produced == consumed == 500 і порожній буфер).

#### Українські демонстраційні дані

Фабричні методи `SoloSinger.CreateNew()`, `VocalGroup.CreateNew()`, `Song.CreateNew()` і `Performance.CreateNew()` використовують спільний набір `UkrainianMusicData`. Учасники та колективи мають українські назви, країна для учасників — Україна, а репертуар складається з українських пісень. Тривалість генерується окремо для демонстрації статистичних операцій LINQ.

### Тести

`MusicContest.Common.Tests` (xUnit):
- CreateAsync: додає елемент (true), duplicate Id → false;
- ReadAsync: правильний елемент, невідомий Id → KeyNotFoundException;
- ReadAllAsync: порожній сервіс → порожня колекція;
- UpdateAsync: оновлення (true), невідомий Id → false;
- RemoveAsync(element): видалення (true), невідомий Id → false;
- пагінація: правильна сторінка, out-of-range, invalid page/amount;
- IEnumerable/foreach по сервісу (snapshot);
- thread-safe parallel create: 8 × 500 = 4 000, Count == 4 000; Parallell.RunAsync: рівно count об'єктів у сервісі (≥1000 за умовою);
- SaveAsync (за FilePath), concurrent SaveAsync: файл валідний, елементів рівно стільки; без FilePath → InvalidOperationException;

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

### Звітні файли та результати перевірок

PDF-звіт формується скриптом `docs/Lab2_report_generator.py` на основі фактичних журналів. `gen_pdf.py` залишено як сумісну команду запуску цього генератора. Звіт: `docs/Lab2_Report.pdf`. Журнали:
`docs/Lab2_build.txt`, `docs/Lab2_test.txt`, `docs/Lab2_output.txt`.

Результати перевірок (фактичні, з журналів):
- `dotnet build MusicContest.sln`: Build succeeded, 0 Warning(s), 0 Error(s) — exit code 0;
- `dotnet test MusicContest.Common.Tests`: Passed! Failed: 0, Passed: 22, Total: 22 — exit code 0 (процес завершився сам, без зависання);
- `dotnet run --project MusicContest.Console`: exit code 0; Parallel.For створив 2000 із 2000 об'єктів через CRUD-сервіс; LINQ по реальній колекції: Min = 120 s, Max = 359 s, Average ≈ 239,18 s; SaveAsync — у файлі 2000 елементів після 3 конкурентних saves; lock Match: True (80000); SemaphoreSlim виміряний max = 3; AutoResetEvent Produced = Consumed = 500.

## Git / GitHub

Робоча гілка ЛР1: `lab1`.
Робоча гілка ЛР2: `lab2` — містить код ЛР1 та ЛР2; розробкова гілка: `lab2-qwen38`.

Готова лабораторна робота №2 подається через Pull Request із гілки `lab2`:
у `master`, або у `lab1`, якщо PR із `lab1` ще не закритий. Фактично відкритий Pull Request #2 `lab2 → lab1` (PR лабораторної №1 ще відкритий): https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/2. Експериментальна гілка `lab2-qwen38` для здачі не використовується.
