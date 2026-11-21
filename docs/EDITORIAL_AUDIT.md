# Звіт редакційного й технічного аудиту лабораторних робіт №1–3

**Дата перевірки:** 22 листопада 2026 року  
**Репозиторій:** MusicContest  
**Перевірені гілки:** `lab1`, `lab2`, `lab3`, `main`.

## Обсяг аудиту

Перевірено README, контрольні запитання, інструкції подання, редакційні правила, звіти, генератори PDF, журнали запуску, коментарі C# та описи Pull Request. Гілка `master` містить лише службовий `.gitignore`, тому навчальні матеріали в ній не редагувалися.

## Редакційні зміни

1. Додано [EDITORIAL_WRITING_STANDARD.md](../EDITORIAL_WRITING_STANDARD.md) і пов'язано його з мовною політикою в усіх робочих гілках.
2. Установлено українську як основну мову авторських матеріалів. Англійська використовується для технічних назв, API та міжнародних матеріалів.
3. Переписано контрольні відповіді ЛР1, ЛР2 та ЛР3. Визначення мають різноманітнішу синтаксичну будову; прибрано повторювані конструкції з довгим тире.
4. Відредаговано README, інструкції подання та описи Pull Request №1, №2 і №4.
5. Уточнено коментарі C# у моделях, CRUD-сервісах, паралельній обробці, синхронізації, SQLite та MongoDB.
6. Переглянуто формулювання в генераторах PDF. Звіти повторно сформовано з фактичних журналів збірки й запуску.
7. Перевірено текстові матеріали на російськомовні фрагменти. У перевірених авторських документах, коментарях і журналах таких фрагментів не виявлено.

## Технічні уточнення

### Лабораторна робота №2

- Уточнено різницю між асинхронністю та багатопотоковістю.
- Зафіксовано фактичну поведінку CRUD: операції над `ConcurrentDictionary` виконуються синхронно, а `Task.Yield()` планує асинхронне продовження.
- Окремо пояснено, що `SaveAsync()` виконує асинхронний файловий запис через `File.WriteAllTextAsync()`.
- Зазначено, що snapshot є поверхневою копією послідовності. Об'єкти посилальних типів не клонуються.
- У демонстрації producer/consumer додано завершальний сигнал `itemReady.Set()` після `producerFinished.Set()`. Це будить споживача, якщо він встиг обробити останній сигнал про дані до завершення виробника.

### Лабораторна робота №3

- Уточнено пояснення зв'язків 1:1, 1:N і M:N та наслідування TPT.
- У ERD позначено поля, що одночасно виконують ролі PK і FK, а також тип зберігання GUID у SQLite.
- Повторно створено SQLite-базу, скриншоти таблиць, ERD і PDF-звіт.
- Перевірено додаткову демонстрацію MongoDB.

## Результати автоматичної перевірки

| Лабораторна | Збірка | Тести | Додаткова перевірка |
|:--|:--|:--|:--|
| №1 | Успішна, 0 попереджень і помилок | Не передбачені окремим тестовим проєктом | Консольний CRUD, подія, JSON |
| №2 | Успішна, 0 попереджень і помилок | **22 із 22** | 2000 об'єктів, JSON, LINQ, синхронізація |
| №3 | Успішна, 0 попереджень і помилок | **27 із 27** | SQLite CRUD, MongoDB CRUD, ERD і скриншоти |

### Фактичний склад демонстраційної SQLite-бази

| Таблиця або зв'язок | Кількість |
|:--|--:|
| Учасники | 6 |
| Сольні виконавці | 4 |
| Вокальні гурти | 2 |
| Пісні | 8 |
| Профілі учасників | 6 |
| Виступи | 12 |
| Записи зв'язку M:N | 12 |

У MongoDB перевірено CRUD-операції та наявність восьми документів українського репертуару.

## Посилання на результати

- [Гілка lab1](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab1) · [PDF ЛР1](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab1/docs/Lab1_Report.pdf) · [Перевірка GitHub Actions](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/actions/runs/44960460923)
- [Гілка lab2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab2) · [PDF ЛР2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab2/docs/Lab2_Report.pdf) · [Перевірка GitHub Actions](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/actions/runs/44960464825)
- [Гілка lab3](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/tree/lab3) · [PDF ЛР3](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab3/docs/Lab3_Report.pdf) · [ERD](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab3/docs/Lab3_ERD.png) · [Перевірка GitHub Actions](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/actions/runs/44960355757)
- [Pull Request №1](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/1) · [Pull Request №2](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/2) · [Pull Request №4](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/pull/4)

## Повторна редакційна перевірка, 22 листопада 2026 року

Переглянуто текстові файли трьох окремих гілок: **17 файлів у `lab1`, 29 у `lab2` та 48 у `lab3`**. До перевірки увійшли Markdown, коментарі C#, Python-генератори PDF, сценарії GitHub Actions і матеріали подання. В авторських текстах не виявлено нових очевидних російськомовних фрагментів. Автоматична перевірка доповнює редакторський огляд, але не є повноцінним визначенням мови для кожного слова.

### Що змінено

| Частина | Зміна |
|:--|:--|
| `lab1` | Додано [короткий звіт із блок-схемою](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab1/docs/PROJECT_REPORT.md), уточнено README та інструкцію подання, скорочено опис PR №1 |
| `lab2` | Додано [короткий звіт](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab2/docs/PROJECT_REPORT.md), уточнено README та інструкцію подання, скорочено опис PR №2 |
| `lab3` | Усунуто повтор у README; два застарілі PDF-генератори замінено сумісними запускними скриптами без старих ненормативних формулювань; спрощено опис PR №4 |
| PDF ЛР1–2 | Налаштовано перенесення довгих рядків консольного виводу, щоб журнали не обрізалися біля правого поля |
| PDF ЛР3 | Виправлено відмінювання слова «запис» після числівників і точніше описано необов'язковий зв'язок профілю з учасником |
| Перевірка PDF | Додано `scripts/check_pdf_language.py`: контроль текстового шару, пошук очевидних іншомовних залишків, створення PNG-прев'ю |

### Перевірка готових PDF

Для кожної гілки повторно сформовано PDF. Зміст перевірено через витягування тексту, а перші сторінки переглянуто як зображення. Для ЛР3 додатково переглянуто сторінку з ERD. Текстовий шар PDF є доступним для пошуку; у ньому не знайдено очевидних російськомовних залишків. Довгі шляхи до файлів переносяться на наступний рядок і зберігають зміст оригінального журналу.

- [ЛР1: PDF](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab1/docs/Lab1_Report.pdf) · [прев'ю](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab1/docs/Lab1_Report_preview.png) · [витягнутий текст](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab1/docs/Lab1_Report_extracted.txt)
- [ЛР2: PDF](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab2/docs/Lab2_Report.pdf) · [прев'ю](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab2/docs/Lab2_Report_preview.png) · [витягнутий текст](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab2/docs/Lab2_Report_extracted.txt)
- [ЛР3: PDF](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab3/docs/Lab3_Report.pdf) · [прев'ю](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab3/docs/Lab3_Report_preview.png) · [сторінка ERD](https://github.com/ZvezniyLord/NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab/blob/lab3/docs/Lab3_Report_preview_erd.png)

Підтверджені результати останніх запусків GitHub Actions: **ЛР1, збірка й запуск успішні; ЛР2, 22/22 тести; ЛР3, 27/27 тестів**. PDF, згенеровані у відповідних гілках, перевірено тим самим процесом. Функціональність програмних моделей і структуру бази даних під час цього редакційного етапу не змінено.

---

## Підсумок

Матеріали трьох лабораторних приведено до спільної редакційної політики. PDF-звіти сформовано повторно. Збірки й автоматизовані перевірки завершилися успішно. Історію гілок не переписували; зміни внесено звичайними комітами.
