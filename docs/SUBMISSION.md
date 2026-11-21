# Подання лабораторної роботи №2

**Тема:** багатопотоковість, асинхронність, IEnumerable та LINQ.  
**Проєкт:** MusicContest, C# / .NET 8.  
**Студент:** Галенко. **Група:** 404TH. **Варіант:** TK.

## Матеріали

- [Гілка lab2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab2)
- [Pull Request №2: lab2 → lab1](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/2)
- [PDF-звіт лабораторної №2](Lab2_Report.pdf)
- [PDF-звіт лабораторної №1](Lab1_Report.pdf)
- [Контрольні запитання](../LAB2_CONTROL_QUESTIONS.md)
- [Редакційна мовна політика](../EDITORIAL_LANGUAGE_POLICY.md)

## Виконані завдання

1. Реалізовано асинхронний узагальнений CRUD-сервіс із підтримкою `IEnumerable<T>`.
2. Для зберігання використано `ConcurrentDictionary<Guid, T>`; для читання сервіс формує впорядкований знімок колекції.
3. Реалізовано пагінацію засобами LINQ та асинхронне збереження JSON, захищене `SemaphoreSlim`.
4. Продемонстровано паралельне створення 2000 об'єктів через `Parallel.For` і `Task.WhenAll`.
5. Реалізовано демонстрації `lock`, `SemaphoreSlim`, `AutoResetEvent`, `Interlocked` та producer/consumer.
6. Додано xUnit-тести й спільний набір українських демонстраційних даних.

## Перевірені результати

- Збірка: успішна, 0 попереджень і 0 помилок.
- Автоматизовані тести: 22 із 22 пройдено.
- Паралельне створення: 2000 із 2000 об'єктів.
- Асинхронне збереження: 2000 елементів у JSON-файлі.
- `lock`: фактичне значення лічильника — 80 000.
- `SemaphoreSlim`: виміряний максимум — 3 одночасні завдання.
- `AutoResetEvent`: створено й оброблено 500 елементів.

Журнали збірки, тестування та запуску збережено в цій папці. PDF-звіт формується з цих фактичних журналів.

## Самостійна перевірка

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln
dotnet test MusicContest.Common.Tests/MusicContest.Common.Tests.csproj
dotnet run --project MusicContest.Console
```

Оскільки Pull Request лабораторної №1 ще відкритий, лабораторна робота №2 подається з гілки `lab2` до `lab1`.
