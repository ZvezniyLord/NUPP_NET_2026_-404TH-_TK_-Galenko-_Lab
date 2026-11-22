# Лабораторна робота №4

Тема: розробка вебсерверів засобами ASP.NET Core.

Основний проєкт `MusicContest.REST` реалізує CRUD для пісень і виступів через асинхронний сервіс із лабораторної №3. Окремі DTO розташовані в `Models`, контролери в `Controllers`. Доступний Swagger UI.

Додатковий проєкт `MusicContest.MVC` відображає HTML-таблиці з наявного `MusicContestContext`.

## Автоматичний запуск у Windows

У корені гілки `lab4` є файл **[`START_LAB4.bat`](../START_LAB4.bat)**. Запусти його подвійним клацанням у Провіднику Windows.

Сценарій:

1. Перевіряє наявність .NET 8 SDK. Якщо SDK відсутній, запускає встановлення офіційного пакета Microsoft через `winget` із можливим запитом прав адміністратора.
2. Відновлює пакети NuGet і збирає рішення в конфігурації Release.
3. Запускає демонстрацію лабораторної №3, застосовує міграції та створює відсутні навчальні записи SQLite. Наявні записи не видаляються.
4. Перевіряє, що порти 5084 і 5085 вільні; відкриває два окремі термінали для REST API та MVC.
5. Дочікується HTTP-відповідей і відкриває обидва застосунки в стандартному браузері.

**Адреси після запуску:**

- Вебсайт MVC: http://127.0.0.1:5085/
- Swagger REST API: http://127.0.0.1:5084/swagger

**Вимоги:** Windows 10/11, доступ до інтернету під час першого завантаження .NET SDK і пакетів NuGet, `winget` (коли SDK ще не встановлений), а також можливість погодити встановлення програмного забезпечення. Git і Python для цього BAT-файлу не потрібні. Установка .NET може потребувати прав адміністратора.

Якщо `winget` відсутній або встановлення не вдалося, встанови [офіційний .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) вручну й запусти BAT повторно. Якщо `dotnet` не визначився відразу після встановлення, закрий термінали та перезапусти BAT.

**Завершення роботи:** закрий два вікна з назвами `MusicContest MVC` та `MusicContest REST API`. Повторний запуск можливий без перевстановлення SDK. Закривати основне вікно BAT необов'язково після відкриття браузера.

## Ручний запуск

```powershell
dotnet restore MusicContest.sln
dotnet build MusicContest.sln -c Release
dotnet run --project MusicContest.Console -c Release -- --lab3-only
```

В окремих терміналах:

```powershell
dotnet run --project MusicContest.MVC -c Release --urls http://127.0.0.1:5085
dotnet run --project MusicContest.REST -c Release --urls http://127.0.0.1:5084
```

Для перевірки HTTP-запитів:

```powershell
python scripts/check_lab4_api.py
python scripts/generate_lab4_report.py
```

Документація: [матеріали для подання](LAB4_SUBMISSION.md), [контрольні запитання](LAB4_CONTROL_QUESTIONS.md).

PDF-звіт `Lab4_Report.pdf` формується лише з фактичних HTTP-відповідей. Наявні результати GitHub Actions перевіряють REST API, а локальний запуск MVC слід перевірити в браузері.
