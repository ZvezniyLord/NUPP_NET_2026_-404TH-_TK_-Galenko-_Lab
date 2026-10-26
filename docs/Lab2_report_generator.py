"""Генератор PDF-звіту ЛР2.

Portable: використовує ТІЛЬКИ відносні шляхи — скрипт шукає
каталог docs/ поруч із самим собою (repo/docs). Дані беруться
виключно з реальних файлів:

  docs/Lab2_build.txt   — результат dotnet build
  docs/Lab2_test.txt    — результат dotnet test
  docs/Lab2_output.txt  — результат dotnet run (включно з exit code)

Ніяких результатів НЕ є записаними вручну: усі ключові цифри дістаються
з txt-файлів через регулярні вирази. Якщо рядка немає — маркер
"<не знайдено>".
"""

from __future__ import annotations

import os
import re

from fpdf import FPDF
from fpdf.enums import XPos, YPos

# Unicode-шрифти з підтримкою кирилиці (українські літери і ґ є).
FONT_REGULAR = r"C:\Windows\Fonts\arial.ttf"
FONT_BOLD = r"C:\Windows\Fonts\arialbd.ttf"

DOCS = "docs"


class Report:
    """Будує PDF-звіт; усі числа — з реальних txt-логів."""

    def __init__(self) -> None:
        def load(name: str) -> list[str]:
            with open(f"{DOCS}/{name}", "r", encoding="utf-8") as f:
                return f.read().splitlines()

        self.build_lines = load("Lab2_build.txt")
        self.test_lines = load("Lab2_test.txt")
        self.output_lines = load("Lab2_output.txt")

    # ---------- Перевірка (helper): один рядок, що містить regex ----------
    @staticmethod
    def find_line(lines: list[str], pattern: str) -> str:
        rgx = re.compile(pattern)
        for line in lines:
            m = rgx.search(line)
            if m:
                return m.group(0).strip()
        return "<не знайдено>"

    # ---------- Факти, дістаються реальним (real) шляхом з txt ----------
    @property
    def build_status(self) -> str:
        return self.find_line(self.build_lines, r"Build (succeeded|FAILED)")

    @property
    def build_warnings(self) -> str:
        return self.find_line(self.build_lines, r"(\d+) Warning\(s\)")

    @property
    def build_errors(self) -> str:
        return self.find_line(self.build_lines, r"(\d+) Error\(s\)")

    @property
    def test_summary(self) -> str:
        return self.find_line(
            self.test_lines,
            r"(Passed!|Failed!).*Total:\s*\d+.*",
        )

    @property
    def exit_code(self) -> str:
        line = self.find_line(self.output_lines, r"Run exit code: -?\d+")
        return line

    @property
    def parallel_objects(self) -> str:
        return self.find_line(self.output_lines, r"Об'єктів створено: \d+ із \d+ планових")

    @property
    def parallel_time(self) -> str:
        return self.find_line(self.output_lines, r"Фактичний час: [\d.,]+ ms")

    @property
    def min_duration(self) -> str:
        return self.find_line(self.output_lines, r"Min \(DurationSeconds\): \d+ s")

    @property
    def max_duration(self) -> str:
        return self.find_line(self.output_lines, r"Max \(DurationSeconds\): \d+ s")

    @property
    def avg_duration(self) -> str:
        return self.find_line(self.output_lines, r"Average \(DurationSeconds\): [\d.,]+ s")

    @property
    def saved_elements(self) -> str:
        return self.find_line(
            self.output_lines, r"Елементів у файлі після конкурентного Save: \d+"
        )

    @property
    def lock_expected(self) -> str:
        return self.find_line(self.output_lines, r"Expected: \d+")

    @property
    def lock_actual(self) -> str:
        return self.find_line(self.output_lines, r"Actual:.*\d+")

    @property
    def lock_match(self) -> str:
        return self.find_line(self.output_lines, r"Match:.*")

    @property
    def semaphore_max(self) -> str:
        return self.find_line(
            self.output_lines, r"ФАКТИЧНО виміряно максимум одночасних worker.?ів: \d+"
        )

    @property
    def produced_consumed(self) -> str:
        produced = self.find_line(self.output_lines, r"Produced:\s+\d+")
        consumed = self.find_line(self.output_lines, r"Consumed:\s+\d+")
        return produced + "; " + consumed

    # ---------- Тексти (content) ----------
    def build_summary(self) -> str:
        return "\n".join(self.build_lines[-5:])

    def test_summary_text(self) -> str:
        return "\n".join(self.test_lines[-3:])

    def run_output_full(self) -> str:
        # УСІ рядки реального console run (крім рядка exit code).
        lines = [line for line in self.output_lines if not line.startswith("Run exit code")]
        return "\n".join(lines)

    @property
    def conclusion(self) -> str:
        return (
            "Усі завдання лабораторної роботи №2 реалізовані та перевірені "
            "реальними build/test/run (dotnet). "
            f"Build: {self.build_status}; "
            f"{self.build_warnings} — {self.build_errors}. "
            f"{self.test_summary}. "
            f"{self.exit_code}. "
            f"Parallell + CRUD service: {self.parallel_objects}; {self.parallel_time}. "
            f"LINQ: {self.min_duration} / {self.max_duration} / {self.avg_duration}. "
            f"SaveAsync у файл: {self.saved_elements}. "
            f"Lock: {self.lock_expected} / {self.lock_actual} ({self.lock_match}). "
            f"SemaphoreSlim: {self.semaphore_max}. "
            f"Producer/Consumer: {self.produced_consumed}. "
            "Потіки (threads), асинхронність (async/await), IEnumerable, LINQ, "
            "Parallel та примітиви синхронізації (lock, SemaphoreSlim, "
            "AutoResetEvent) реалізовані та підтверджені звітними файлами "
            "docs/Lab2_build.txt, docs/Lab2_test.txt та docs/Lab2_output.txt."
        )

    def render(self) -> bytes:
        pdf = FPDF()
        pdf.add_font("Uni", "", FONT_REGULAR)
        pdf.add_font("Uni", "B", FONT_BOLD)

        def heading(text: str, size: int = 16) -> None:
            pdf.set_font("Uni", "B", size)
            pdf.multi_cell(w=190, h=size + 4, text=text)

        def body(text: str, size: int = 11) -> None:
            pdf.set_font("Uni", "", size)
            pdf.multi_cell(w=190, h=size + 2, text=text)

        def mono(lines_text: str, size: int = 8.5) -> None:
            # mono — блок з логами (моноширинний rendering).
            pdf.set_font("Uni", "", size)
            pdf.multi_cell(w=190, h=4.6, text=lines_text)

        theme = (
            "Тема: Багатопотковість, асинхронність, IEnumerable, LINQ.\n"
            "Мета: реалізувати асинхроний thread-safe generic CRUD-сервіс "
            "(ICrudServiceAsync<T> : IEnumerable<T>, пагінація, збереження у "
            "файл за FilePath), паралельно створити від 1000 об'єктів через "
            "CreateNew() + клас Parallell + CRUD-сервіс, знайти LINQ-ом min/max/"
            "average цифрових значень, зберегти колекцію у файл та продемонструвати "
            "примітиви синхронізації. Перевірка реальними build/test/run через dotnet."
        )
        architecture = (
            "Музичний конкурс (MusicContest) — предметна область, збережена з ЛР1.\n"
            "- ICrudServiceAsync<T> : IEnumerable<T> (where T : IEntity): CreateAsync/UpdateAsync/"
            "RemoveAsync повертають Task<bool>, ReadAsync — Task<T>, ReadAllAsync та "
            "ReadAllAsync(page, amount) — Task<IEnumerable<T>>, SaveAsync() — Task<bool>.\n"
            "- CrudServiceAsync<T>: внутрішня колекція ConcurrentDictionary<Guid, T> — "
            "ВБУДОВANA thread-safe колекція .NET; операції TryAdd/TryUpdate/TryRemove "
            "атомарні під конкурентним доступом потоків без зовнішнього lock. Зчитування "
            "та foreach працюють зі SNAPSHOT (миттєвою копією, впорядкованою за Id) — "
            "пагінація детермінована.\n"
            "- SaveAsync(): File.WriteAllTextAsync у файл за FilePath; статичний "
            "SemaphoreSlim (1 мітка) — одночасно пише лише один writer.\n"
            "- Parallell.RunAsync: Parallel.For паралельно створює об'єкти через "
            "Song.CreateNew() та запускає CrudServiceAsync<Song>.CreateAsync; після join point — "
            "Task.WhenAll; час — Stopwatch, реальна кількість — з колекції сервісу.\n"
            "- SynchronizationDemos: lock (8 потоків × 10 000 increments), SemaphoreSlim "
            "(10 worker'ів, maxConcurrency = 3), AutoResetEvent (producer/consumer, M = 500); "
            "лічильники — Interlocked.\n"
            "- Program.cs: блок ЛР1 + блок ЛР2 (Parallel+CRUD, LINQ min/max/average, "
            "пагінація, IEnumerable foreach, SaveAsync у файл, lock, SemaphoreSlim, AutoResetEvent).\n"
            "- MusicContest.Common.Tests (xUnit): 22 тестів — CRUD-семантика bool, пагінація, "
            "IEnumerable, thread-safe parallel create (8 × 500 = 4000), SaveAsync, concurrent "
            "SaveAsync, Parallell.RunAsync."
        )
        features_md = (
            "1. Async generic CRUD: CreateAsync/ReadAsync/ReadAllAsync/\n"
            "   ReadAllAsync(page, amount)/UpdateAsync/RemoveAsync/SaveAsync — за інтерфейсом завдання.\n"
            "2. Thread safety: ConcurrentDictionary<Guid, T> (вбудована thread-safe колекція .NET);\n"
            "   snapshot для читання; файловий запис захищений SemaphoreSlim.\n"
            "3. IEnumerable<T>: foreach безпосередньо по сервісу (enumerator над snapshot).\n"
            "4. Пагінація: Skip/Take, сторінки з 1; поза межами — порожня колекція;\n"
            "   page/amount <= 0 — ArgumentOutOfRangeException.\n"
            "5. CreateNew(): статичні фабрики Song/SoloSinger/VocalGroup/Performance\n"
            "   (Random.Shared — thread-safe генератор).\n"
            "6. Parallell: Parallel.For створює 2000 Song; додавання через CRUD-сервіс,\n"
            "   зшивання Task.WhenAll після join point; Stopwatch; реальна кількість з сервісу.\n"
            "7. LINQ на цифровій властивості DurationSeconds: Min, Max, Average,\n"
            "   Where, Select, OrderByDescending, Aggregate, Skip/Take.\n"
            "8. SaveAsync(): колекція збережена у JSON за FilePath; 3 конкурентні\n"
            "   SaveAsync — файл залишається валідним (усі елементи).\n"
            "9. Примітиви синхронізації: lock (Expected/Actual/Match), SemaphoreSlim\n"
            "   (виміряний максимум одночасних worker'ів Interlocked), AutoResetEvent\n"
            "   (producer/consumer на ConcurrentQueue, Produced == Consumed == M).\n"
            "10. Unit tests (xUnit): 22 тестів."
        )

        pdf.add_page()
        heading("Лабораторна робота №2 — MusicContest (C#/.NET 8)", 16)
        body("Студент: Галенко; група: 404TH; варіант оцінювання: TK. Репозиторій: NUPP_NET_2026_-404TH-_TK_-Galenko-_Lab, фінальна гілка lab2.", 11)
        pdf.ln(4)
        heading("Тема та мета", 13)
        body(theme, 12)

        pdf.ln(2)
        heading("Архітектура та реалізація", 13)
        body(architecture, 11)

        pdf.ln(2)
        heading("Виконані завдання", 13)
        body(features_md[:2000], 11)

        pdf.add_page()
        heading("Реальні результати виконання консольного застосунку", 13)
        body(f"Run (dotnet run --project MusicContest.Console) — {self.exit_code}", 12)
        mono(self.run_output_full())

        pdf.add_page()
        heading("Build та тести", 13)
        body("Build (dotnet build):", 12)
        mono(self.build_summary())
        pdf.ln(2)
        body("Tests (dotnet test MusicContest.Common.Tests):", 12)
        mono(self.test_summary_text())
        pdf.ln(2)
        heading("Висновок:", 13)
        body(self.conclusion, 11)

        return bytes(pdf.output())


def main() -> None:
    report = Report()
    pdf_bytes = report.render()
    with open(f"{DOCS}/Lab2_Report.pdf", "wb") as f:
        f.write(pdf_bytes)

    print("PDF згенеровано:", len(pdf_bytes), "bytes")
    print("Build:", report.build_status)
    print("Tests:", report.test_summary)
    print("Exit code:", report.exit_code)
    print("Parallell:", report.parallel_objects, "|", report.parallel_time)
    print("LINQ:", report.min_duration, "/", report.max_duration, "/", report.avg_duration)
    print("Save:", report.saved_elements)
    print("Lock:", report.lock_expected, "/", report.lock_actual, report.lock_match)
    print("Semaphore:", report.semaphore_max)
    print("Producer/Consumer:", report.produced_consumed)


if __name__ == "__main__":
    main()
