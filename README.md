# Лабораторна робота №1 — MusicContest

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

Робоча гілка: `lab1`.

Готова лабораторна робота подається через Pull Request `lab1 -> master`.
