# Лабораторна робота №3 — відповіді на контрольні запитання

**Тема:** бази даних у .NET, Entity Framework Core. **Предметна область:** MusicContest (музичний конкурс).

### 1. Що таке реляційні БД, СУБД? Приклади
Реляційна база даних представляє інформацію як множину взаємопов'язаних таблиць. Таблиці мають рядки (записи), стовпці (атрибути), ключі та обмеження цілісності. СУБД — програмна система для створення, читання, оновлення, адміністрування й захисту баз даних. Приклади реляційних СУБД: SQLite, PostgreSQL, Microsoft SQL Server, MySQL/MariaDB, Oracle Database.

### 2. Що таке таблиця? Які існують зв'язки?
Таблиця описує однотипні сутності: рядок — один об'єкт, стовпець — його атрибут. Первинний ключ (PK) ідентифікує запис, зовнішній ключ (FK) посилається на запис іншої таблиці. Типи зв'язків: один-до-одного (1:1), один-до-багатьох (1:N) і багато-до-багатьох (M:N). Зв'язок M:N зазвичай реалізують проміжною таблицею з двома FK.

### 3. Що таке ERD, як і навіщо створюється?
ERD (Entity Relationship Diagram) — графічна схема таблиць, їхніх полів, ключів та зв'язків. Її створюють за моделлю даних у DB Browser, DBeaver, dbdiagram.io, Mermaid або інших інструментах. ERD потрібна для перевірки нормалізації, кардинальностей та правильної організації FK. У MusicContest ERD відображає сім таблиць предметної області.

### 4. Що таке DbContext?
`DbContext` — центральний клас EF Core, який координує запити, відстеження змін, мапінг моделей на таблиці та `SaveChangesAsync()`. Наш `MusicContestContext` задає шість `DbSet` і відношення через `OnModelCreating`. Для SQLite підключення налаштовано в `MusicContestDatabase.Options()`.

### 5. Поясніть 1:1, 1:N, M:N в EF Core
**1:1:** одному учаснику відповідає максимум один профіль (ContestParticipantModel → ParticipantProfileModel), FK профілю є унікальним. **1:N:** один учасник має багато виступів, кожен виступ посилається на одного учасника (PerformanceModel.ParticipantDatabaseId). **M:N:** учасник може обрати кілька пісень, а пісню — кілька учасників; їх пов'язує таблиця ParticipantSongs.

### 6. Приклад 1:1 у Fluent API
```csharp
modelBuilder.Entity<ParticipantProfileModel>()
    .HasOne(profile => profile.Participant)
    .WithOne(participant => participant.Profile)
    .HasForeignKey<ParticipantProfileModel>(profile => profile.ParticipantDatabaseId);
```
Унікальний індекс FK гарантує, що один учасник не матиме двох профілів.

### 7. Приклад M:N у EF Core
```csharp
modelBuilder.Entity<ContestParticipantModel>()
    .HasMany(participant => participant.Songs)
    .WithMany(song => song.Participants)
    .UsingEntity<Dictionary<string, object>>(
        "ParticipantSongs",
        right => right.HasOne<SongModel>().WithMany().HasForeignKey("SongDatabaseId"),
        left => left.HasOne<ContestParticipantModel>().WithMany().HasForeignKey("ParticipantDatabaseId"),
        join => join.HasKey("ParticipantDatabaseId", "SongDatabaseId"));
```
EF Core формує таблицю зв'язку з комбінованим PK.

### 8. Data Annotations проти Fluent API
**Data Annotations** — атрибути на властивостях C# (`[Required]`, `[MaxLength]`, `[Key]`). Вони прості для базових правил, але змішують правила зберігання з моделями. **Fluent API** — налаштування у `OnModelCreating`; дає ширші можливості (TPT, складні індекси, правила каскадного видалення, M:N, типи колонок) та тримає конфігурацію окремо. У нашому проєкті застосовано Fluent API.

### 9. Як створити та застосувати міграцію?
Типовий робочий процес: встановити `dotnet-ef`, виконати `dotnet ef migrations add InitialCreate --project MusicContest.Infrastructure --startup-project MusicContest.Console`, потім `dotnet ef database update --project MusicContest.Infrastructure --startup-project MusicContest.Console`. Міграції містять методи `Up()`/ `Down()`, а історію застосування EF записує до `__EFMigrationsHistory`. У ЛР3 початкова міграція вже додана у `Migrations/`, а `Database.MigrateAsync()` застосовує її при старті консолі.

### 10. Призначення MusicContest.Infrastructure
Це інфраструктурний шар: моделі збереження, `DbContext`, конфігурація SQLite, міграції, `EfRepository<T>` і адаптер CRUD. `MusicContest.Common` описує домен та абстрактний `IRepository<T>`, тому не залежить від конкретної СУБД. Консольний проєкт використовує інфраструктуру для доступу до реальних даних.

### 11. Доменна модель проти моделі БД
Доменна модель `SoloSinger`, `VocalGroup`, `Song`, `Performance` із ЛР1 реалізує поведінку конкурсу (події, методи, логіка). EF-моделі із суфіксом `Model` відображаються на таблиці: містять `DatabaseId`, FK та навігаційні властивості. Поділ запобігає жорсткій прив'язці предметної логіки до способу збереження.

### 12. Навіщо Repository Pattern?
Repository ізолює доступ до БД від бізнес-логіки: CRUD-сервіс залежить від `IRepository<T>`, а не від `DbContext` або SQL. Це допомагає тестуванню, заміні реалізації зберігання, централізації базових операцій. У ЛР3 реалізація `EfRepository<T>` використовує EF Core.

### 13. Головна різниця між реляційними й нереляційними БД
Реляційні БД зазвичай мають таблиці, фіксовану схему та SQL, підтримують FK та JOIN. Нереляційні БД застосовують інші моделі (документи, ключ-значення, графи, колонки) і часто дозволяють гнучку структуру записів. Приклад документної БД — MongoDB; вона може зберігати вкладені об'єкти без нормалізації на окремі таблиці. Вибір залежить від вимог до цілісності, запитів і масштабування.

---
**Примітка до захисту:** база у `docs/musiccontest.db`, назви таблиць: ContestParticipants, SoloSingers, VocalGroups, ParticipantProfiles, Songs, Performances, ParticipantSongs; службова таблиця EF: `__EFMigrationsHistory`.
