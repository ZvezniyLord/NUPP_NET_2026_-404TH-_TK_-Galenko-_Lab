# Подання лабораторної роботи №2

**Тема:** багатопотоковість, асинхронність, IEnumerable та LINQ.  
**Проєкт:** MusicContest, C# / .NET 8.  
**Студент:** Галенко. **Група:** 404TH. **Варіант:** TK.

## Матеріали

- [Гілка lab2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab2)
- [Pull Request №2: lab2 → lab1](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/2)
- [PDF-звіт лабораторної №2](Lab2_Report.pdf)
- [Журнал збірки](Lab2_build.txt)
- [Журнал тестування](Lab2_test.txt)
- [Журнал роботи консольного застосунку](Lab2_output.txt)
- [Відповіді на контрольні запитання](../LAB2_CONTROL_QUESTIONS.md)

## Виконані завдання

1. Реалізовано асинхронний узагальнений CRUD-сервіс із підтримкою `IEnumerable<T>`.
2. Використано `ConcurrentDictionary<Guid, T>` та знімок колекції для безпечного читання.
3. Реалізовано пагінацію засобами LINQ і асинхронне збереження JSON.
4. Продемонстровано паралельне створення 2000 об'єктів через `Parallel.For` та `Task.WhenAll`.
5. Реалізовано демонстрації `lock`, `SemaphoreSlim`, `AutoResetEvent`, `Interlocked` і producer/consumer.
6. Додано модульні тести xUnit та спільний набір українських демонстраційних даних.

## Перевірка

Усі результати збірки, тестування та запуску записуються автоматично до журналів у папці `docs`. PDF формується скриптом `docs/Lab2_report_generator.py` без ручного введення показників.

Команди для самостійної перевірки:

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet test MusicContest.Common.Tests/MusicContest.Common.Tests.csproj
dotnet run --project MusicContest.Console
```

Експериментальна гілка `lab2-qwen38` не входить до складу подання лабораторної роботи №2.
