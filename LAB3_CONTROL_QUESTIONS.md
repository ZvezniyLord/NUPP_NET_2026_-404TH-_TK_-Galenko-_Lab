# Лабораторна робота №3. Відповіді на контрольні запитання

**Тема:** бази даних у .NET та Entity Framework Core.  
**Предметна область:** MusicContest, модель українського вокального конкурсу.

### 1. Що таке реляційні бази даних і СУБД? Наведіть приклади.

Реляційна база даних зберігає інформацію в таблицях, пов'язаних між собою ключами. Таблиці складаються з рядків і стовпців; для записів визначають ключі та обмеження цілісності. Система керування базами даних (СУБД) призначена для створення, читання, оновлення, адміністрування й захисту даних. До реляційних СУБД належать SQLite, PostgreSQL, Microsoft SQL Server, MySQL/MariaDB та Oracle Database.

### 2. Що таке таблиця? Які зв'язки використовують у реляційних БД?

Таблиця зберігає записи одного типу. Кожен рядок відповідає окремому запису, а стовпці містять його атрибути. Первинний ключ (PK) однозначно ідентифікує запис. Зовнішній ключ (FK) посилається на запис іншої таблиці. У реляційних базах даних використовують зв'язки один-до-одного (1:1), один-до-багатьох (1:N) і багато-до-багатьох (M:N). Останній зазвичай реалізують через проміжну таблицю з двома зовнішніми ключами.

### 3. Що таке ERD, як її створити та для чого вона потрібна?

ERD (Entity Relationship Diagram) показує таблиці, їхні поля, ключі та зв'язки. Схему можна побудувати за моделлю даних у DB Browser, DBeaver, dbdiagram.io, Mermaid або іншому інструменті. Вона допомагає перевірити структуру БД, кардинальність зв'язків і розміщення зовнішніх ключів. ERD проєкту MusicContest містить сім таблиць предметної області.

### 4. Що таке DbContext?

Клас <code>DbContext</code> координує роботу Entity Framework Core з базою даних. Він виконує запити, відстежує зміни сутностей, зіставляє моделі з таблицями та зберігає зміни через <code>SaveChangesAsync()</code>. У проєкті клас <code>MusicContestContext</code> оголошує шість наборів <code>DbSet</code> і налаштовує зв'язки в методі <code>OnModelCreating</code>. Параметри підключення до SQLite визначає <code>MusicContestDatabase.Options()</code>.

### 5. Поясніть зв'язки 1:1, 1:N і M:N в EF Core.

**1:1.** Один учасник може мати не більше одного профілю. Зв'язок між <code>ContestParticipantModel</code> і <code>ParticipantProfileModel</code> налаштовано через зовнішній ключ профілю. EF Core створює для нього унікальне обмеження.

**1:N.** Один учасник може мати кілька виступів. Кожен запис <code>PerformanceModel</code> посилається на одного учасника через <code>ParticipantDatabaseId</code>.

**M:N.** Учасник може обрати кілька пісень, а ту саму пісню можуть виконувати різні учасники. Зв'язок зберігається в проміжній таблиці <code>ParticipantSongs</code>.

### 6. Як налаштувати зв'язок 1:1 за допомогою Fluent API?

    modelBuilder.Entity<ParticipantProfileModel>()
        .HasOne(profile => profile.Participant)
        .WithOne(participant => participant.Profile)
        .HasForeignKey<ParticipantProfileModel>(profile => profile.ParticipantDatabaseId);

Виклик <code>WithOne</code> задає зв'язок один-до-одного. EF Core створює унікальне обмеження для зовнішнього ключа, тому одному учаснику не можна призначити два профілі.

### 7. Наведіть приклад зв'язку M:N в EF Core.

    modelBuilder.Entity<ContestParticipantModel>()
        .HasMany(participant => participant.Songs)
        .WithMany(song => song.Participants)
        .UsingEntity<Dictionary<string, object>>(
            "ParticipantSongs",
            right => right.HasOne<SongModel>().WithMany().HasForeignKey("SongDatabaseId"),
            left => left.HasOne<ContestParticipantModel>().WithMany().HasForeignKey("ParticipantDatabaseId"),
            join => join.HasKey("ParticipantDatabaseId", "SongDatabaseId"));

Конфігурація створює проміжну таблицю. Її складений первинний ключ утворюють два зовнішні ключі.

### 8. Чим відрізняються Data Annotations і Fluent API?

Data Annotations задають правила за допомогою атрибутів у моделях C#, наприклад <code>[Required]</code>, <code>[MaxLength]</code> і <code>[Key]</code>. Такий спосіб зручний для простих обмежень, але пов'язує правила збереження з класами моделей.

Fluent API налаштовує схему в методі <code>OnModelCreating</code>. Він підтримує складні зв'язки, індекси, каскадне видалення, M:N та стратегії наслідування. У проєкті MusicContest використано Fluent API.

### 9. Як створити та застосувати міграцію EF Core?

Спочатку встановлюють інструмент <code>dotnet-ef</code>. Потім створюють міграцію:

    dotnet ef migrations add InitialCreate --project MusicContest.Infrastructure --startup-project MusicContest.Console

Для застосування змін до бази даних виконують:

    dotnet ef database update --project MusicContest.Infrastructure --startup-project MusicContest.Console

У міграції метод <code>Up()</code> описує створення або зміну схеми, а <code>Down()</code> містить зворотні дії. EF Core зберігає відомості про застосовані міграції в таблиці <code>__EFMigrationsHistory</code>. У цій лабораторній початкова міграція міститься в каталозі <code>Migrations/</code>; під час запуску консольного застосунку її застосовує <code>Database.MigrateAsync()</code>.

### 10. Для чого потрібен проєкт MusicContest.Infrastructure?

Цей проєкт містить інфраструктурний код: моделі збереження, <code>DbContext</code>, конфігурацію SQLite, міграції, <code>EfRepository&lt;T&gt;</code> та адаптер CRUD-сервісу. Проєкт <code>MusicContest.Common</code> описує предметну модель і контракт <code>IRepository&lt;T&gt;</code>, не прив'язуючи його до конкретної СУБД. Консольний застосунок використовує інфраструктурний шар для роботи з базою даних.

### 11. Чим доменна модель відрізняється від моделі бази даних?

Доменні класи <code>SoloSinger</code>, <code>VocalGroup</code>, <code>Song</code> і <code>Performance</code> описують об'єкти та поведінку предметної області. EF-моделі із суфіксом <code>Model</code> призначені для збереження даних. Вони містять <code>DatabaseId</code>, зовнішні ключі та навігаційні властивості. Такий поділ не дає деталям конкретної СУБД проникати в доменну логіку.

### 12. Яку проблему розв'язує Repository Pattern?

Шаблон Repository відокремлює доступ до даних від логіки сервісу. CRUD-сервіс працює через інтерфейс <code>IRepository&lt;T&gt;</code>, а не звертається безпосередньо до <code>DbContext</code> або SQL. Це спрощує тестування та дає змогу замінити спосіб зберігання без зміни коду, який використовує контракт. У ЛР3 реалізацію для EF Core надає <code>EfRepository&lt;T&gt;</code>.

### 13. У чому різниця між реляційними та нереляційними базами даних?

Реляційні БД зберігають дані в таблицях із визначеною схемою та підтримують зв'язки через ключі й операції JOIN. Нереляційні системи використовують інші моделі зберігання: документи, пари ключ-значення, графи або колонки. Наприклад, MongoDB зберігає документи BSON із вкладеними об'єктами. Вибір залежить від структури даних, типів запитів, вимог до цілісності та масштабування.

---

**Для захисту:** файл бази даних розміщено в <code>docs/musiccontest.db</code>. У ньому є таблиці <code>ContestParticipants</code>, <code>SoloSingers</code>, <code>VocalGroups</code>, <code>ParticipantProfiles</code>, <code>Songs</code>, <code>Performances</code> і <code>ParticipantSongs</code>. EF Core також створює службову таблицю <code>__EFMigrationsHistory</code>.
