# Лабораторна робота №4

Тема: розробка вебсерверів засобами ASP.NET Core.

Основний проєкт `MusicContest.REST` реалізує CRUD для пісень і виступів через асинхронний сервіс із лабораторної №3. Додатковий проєкт `MusicContest.MVC` показує HTML-таблиці на основі спільної SQLite-бази даних.

## Файли автоматичного запуску

| Операційна система | Файл | Призначення |
|---|---|---|
| **Windows 10/11** | **[`START_LAB4.bat`](../START_LAB4.bat)** | Встановлює відсутній .NET 8 SDK через `winget`, збирає рішення, підготовлює SQLite, запускає сайт MVC і Swagger |
| **MacBook (macOS, Apple Silicon / Intel)** | **[`START_LAB4_MAC.command`](../START_LAB4_MAC.command)** | Встановлює відсутній .NET 8 SDK через офіційний скрипт Microsoft, збирає рішення, підготовлює SQLite, запускає сайт MVC і Swagger |

Обидва файли розташовані в корені гілки `lab4`. **Не потрібно запускати обидва**: виберіть файл відповідно до операційної системи.

## Windows: START_LAB4.bat

Двічі клацніть `START_LAB4.bat` у Провіднику Windows. Сценарій перевіряє SDK, відновлює NuGet-пакети, збирає рішення, запускає демонстрацію лабораторної №3 та перевіряє доступність MVC і API. Для встановлення SDK можуть знадобитися інтернет, `winget` та підтвердження дозволу адміністратора.

Щоб зупинити сервери, закрийте окремі вікна `MusicContest MVC` і `MusicContest REST API`.

## MacBook: START_LAB4_MAC.command

На macOS зручніше завантажити **весь репозиторій гілки `lab4`**, а не лише один `.command`-файл.

1. Відкрийте `START_LAB4_MAC.command` подвійним клацанням у Finder. Якщо macOS блокує запуск або файл не має дозволу на виконання, відкрийте Terminal у корені проєкту та введіть:

   ```bash
   chmod +x START_LAB4_MAC.command
   ./START_LAB4_MAC.command
   ```

2. Якщо .NET 8 SDK відсутній, підтвердьте завантаження офіційного інсталятора Microsoft. SDK встановлюється в `~/.dotnet` для поточного користувача без `sudo` і без Homebrew.
3. Дочекайтеся відновлення NuGet, збірки й підготовки SQLite. Відкриються сторінки MVC і Swagger.
4. Залишайте Terminal відкритим протягом демонстрації. Натисніть **Control+C**, щоб зупинити сервери.

Якщо Gatekeeper перешкоджає відкриттю завантаженого скрипту, використайте системний діалог дозволу лише після перевірки вмісту файлу та походження репозиторію. Скрипт завантажує інсталятор із `https://dot.net/v1/dotnet-install.sh`. Для першого встановлення SDK і пакетів NuGet потрібен інтернет.

**Адреси для обох систем:**

- MVC: http://127.0.0.1:5085/
- Swagger REST API: http://127.0.0.1:5084/swagger

Перший запуск може тривати довше через завантаження залежностей. Під час повторних запусків встановлювати SDK знову не потрібно. Існуючі навчальні дані SQLite не видаляються.

## Ручний запуск

```bash
dotnet restore MusicContest.sln
dotnet build MusicContest.sln -c Release
dotnet run --project MusicContest.Console -c Release -- --lab3-only
```

У двох окремих терміналах:

```bash
dotnet run --project MusicContest.MVC -c Release --urls http://127.0.0.1:5085
dotnet run --project MusicContest.REST -c Release --urls http://127.0.0.1:5084
```

Документація: [подання](LAB4_SUBMISSION.md), [контрольні запитання](LAB4_CONTROL_QUESTIONS.md).

PDF-звіт `Lab4_Report.pdf` формується лише з фактичних HTTP-відповідей. GitHub Actions перевіряє REST API на Linux; запуск Windows BAT та macOS command необхідно перевіряти на відповідних операційних системах.
