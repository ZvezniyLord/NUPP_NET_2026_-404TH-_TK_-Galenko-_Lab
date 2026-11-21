# Лабораторна робота №1: MusicContest

> Мова матеріалів: українська; англійська використовується для технічних назв і міжнародних матеріалів. [Мовна політика](EDITORIAL_LANGUAGE_POLICY.md) · [Редакційний стандарт](EDITORIAL_WRITING_STANDARD.md).

Предметна область: український вокальний конкурс.

## Структура

- `MusicContest.Common`: моделі, CRUD-сервіс, метод розширення.
- `MusicContest.Console`: демонстрація роботи CRUD, статичних конструкцій, делегата, події та серіалізації.
- `docs/PROJECT_REPORT.md`: короткий звіт із блок-схемою та посиланнями на результати.
- `docs/Lab1_Report.pdf`: PDF-звіт із фактичним виводом консольного застосунку.
- `docs/SUBMISSION.md`: короткий опис виконання та матеріали для подання.
- `docs/Lab1_build.txt` і `docs/Lab1_output.txt`: журнали збірки та запуску, з яких формується PDF-звіт.
- `scripts/generate_lab1_report.py`: генератор PDF-звіту, який використовує фактичні журнали.
- `EDITORIAL_WRITING_STANDARD.md`: правила природного авторського викладу.

## Класи моделі

- `ContestParticipant`: абстрактний базовий клас.
- `SoloSinger : ContestParticipant`: сольний виконавець.
- `VocalGroup : ContestParticipant`: вокальний гурт.
- `Song`: конкурсна пісня.
- `Performance`: конкурсний виступ.

У моделі використано конструктори, статичне поле, статичний конструктор, звичайні й статичні методи, делегат, подію та метод розширення.

## CRUD

`CrudService<T>` працює з будь-яким типом, який реалізує `IEntity`. Дані зберігаються у `List<T>`. Реалізовано `Create`, `Read`, `ReadAll`, `Update`, `Remove`.

Додатково реалізовано `Save(string filePath)` та `Load(string filePath)` із JSON-серіалізацією через `System.Text.Json`.

## Запуск

```bash
dotnet restore
dotnet run --project MusicContest.Console
```

## Звіт і перевірка

PDF формується автоматично з фактичних журналів збірки та запуску. У звіті наведено мету, реалізовані можливості, результати `dotnet build` і консольний вивід.

- [Оформлений звіт із блок-схемою](docs/PROJECT_REPORT.md)
- [PDF-звіт](docs/Lab1_Report.pdf)
- [Журнал збірки](docs/Lab1_build.txt)
- [Результат запуску](docs/Lab1_output.txt)

## Git / GitHub

Робоча гілка: `lab1`.

Готова лабораторна робота подається через Pull Request `lab1 -> master`.
