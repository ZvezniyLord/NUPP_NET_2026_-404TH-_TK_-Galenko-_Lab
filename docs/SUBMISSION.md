# Подання лабораторної роботи №2

**Лабораторна робота №2.** Багатопотоковість. Асинхронність. IEnumerable. LINQ.
Проєкт: MusicContest (C#/.NET 8). Студент: Галенко, група 404TH (варіант TK).

## Посилання

- Фінальна гілка: https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab2
- Pull Request (lab2 → lab1, лабораторна №1 ще не об'єднана): https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/2
- PDF-звіт: docs/Lab2_Report.pdf (у гілці lab2)

## Інструкція перевірки

1. Клонувати репозиторій та перевірити гілку `lab2`:
   `git clone https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab.git` , `git checkout lab2`.
2. Зібрати: `dotnet restore` та `dotnet build MusicContest.sln` — очікуваний код завершення 0, 0 помилок.
3. Тести: `dotnet test MusicContest.Common.Tests` — очікуваний код завершення 0; Expected: Passed 22 / Failed 0.
4. Запуск: `dotnet run --project MusicContest.Console` — код завершення 0; у консолі видно Parallel.For (2000 із 2000 об'єктів через CRUD-сервіс), LINQ Min/Max/Average по реально створеній колекції, пагінацію, IEnumerable foreach, конкурентне SaveAsync (у JSON 2000 елементів), демо lock/SemaphoreSlim/AutoResetEvent.
5. Журнали та звіт у папці `docs`: Lab2_build.txt, Lab2_test.txt, Lab2_output.txt, Lab2_Report.pdf.

## Що подається в дистанційну систему

- Посилання на Pull Request (PR #2, гілка `lab2`).
- Файл `Lab2_Report.pdf` із папки `docs` фінальної гілки.
- Відомості: прізвище студента (Галенко), група (404TH), дисципліна (.NET / НУПП).

Примітка: експериментальна гілка `lab2-qwen38` не є предметом здачі; її PR (#3) залишається відкритим і не об'єднується без окремого рішення.
