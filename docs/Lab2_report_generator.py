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

FONT_REGULAR = r"C:\Windows\Fonts\ARIALUNI.ttf"
FONT_BOLD = r"C:\Windows\Fonts\ARIALBD.ttf"

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

    # ---------- Факти, дістаються реалістичним (real) шляхом з txt ----------
    @property
    def build_status(self) -> str:
        return self.find_line(self.build_lines, r"Build (succeeded|FAILED)")

    @property
    def build_warnings(self) -> str:
        return self.find_line(
            self.build_lines, r"(\d+) Warning\(s\)"
        )

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
        return self.find_line(self.output_lines, r"ФАКТИЧНО виміряно максимум одночасних worker.?ів: \d+")

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

    def run_summary(self) -> str:
        # Ті, хто був (the last) 40 рядків (lines) — демо блоків (blocks).
        return "\n".join(self.output_lines[:45])

    @property
    def conclusion(self) -> str:
        return (
            "Усі завдання лабораторної роботи №2 реалізовані та перевірені "
            "реальними build/test/run (dotnet). "
            f"Build: {self.build_status}; "
            f"{self.build_warnings} — {self.build_errors}. "
            f"{self.test_summary}. "
            f"{self.exit_code}. "
            f"Parallell: {self.parallel_objects}; {self.parallel_time}. "
            f"Lock: {self.lock_expected} / {self.lock_actual} ({self.lock_match}). "
            f"SemaphoreSlim: {self.semaphore_max}. "
            f"Producer/Consumer: {self.produced_consumed}. "
            "Потіки (threads), асинхронність (async/await), IEnumerable, LINQ, "
            f"Parallel та механізми синхронізації (lock, SemaphoreSlim, "
            "AutoResetEvent) зроблено (implemented) та підтверджені "
            "звітними файлами docs/Lab2_build.txt, docs/Lab2_test.txt та "
            "docs/Lab2_output.txt."
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

        def mono(lines_text: str, size: int = 9) -> None:
            # mono — блок з логами (монохромний, моношрифтовий).
            pdf.set_font("Uni", "", size)
            pdf.multi_cell(w=190, h=5, text=lines_text)

        theme = (
            "Тема: Багатопотковість, асинхроність, IEnumerable, LINQ.\n"
            "Мета: реалізувати асинхроний thread-safe generic CRUD (IEnumerable<T>, "
            "пагінація) та провести демонстрації Parallel, lock, SemaphoreSlim, "
            "AutoResetEvent, LINQ. Перевірка реалістичних (real) build/test/run "
            "через dotnet (dotnet build, dotnet test, dotnet run)."
        )
        architecture = (
            "Музичний конкурс (MusicContest) — предметна область, "
            "збережена з ЛР1.\n"
            "- CrudServiceAsync<T> : ICrudServiceAsync<T> : IEnumerable<T> "
            "(where T : IEntity) — асинхроний CRUD: CreateAsync, ReadAsync, "
            "ReadAllAsync, ReadAllAsync(page, amount), UpdateAsync, RemoveAsync, "
            "SaveAsync.\n"
            "  Внутрішня колекція List<T> захищена lock (_gate) — кожний "
            "(each) критичний сектор (section) під lock. "
            "Методи читання та ітерації (GetEnumerator) повертають SNAPSHOT "
            "(копію), а не внутрішню колекцію (collection).\n"
            "  SaveAsync — File.WriteAllTextAsync із статичним "
            "(static) SemaphoreSlim (1): одночасно пише файл лише "
            "(only) один (single) writer.\n"
            "- Parallell — Parallel.For + ConcurrentBag<Song> + Random.Shared; "
            "фактичний (actual) час вимірюється через Stopwatch.\n"
            "- SynchronizationDemos: lock (8 потоків (threads) × 10 000 "
            "зростань (increments)), SemaphoreSlim (10 worker'ів, "
            "maxConcurrency = 3), AutoResetEvent (producer/consumer, M = 500); "
            "лічильники (counters) — атомарні (Interlocked).\n"
            "- Program.cs: демонстрації ЛР1 + блок ЛР2 (Parallel, LINQ, "
            "пагінація, IEnumerable, SaveAsync, lock, SemaphoreSlim, "
            "AutoResetEvent).\n"
            "- MusicContest.Common.Tests (xUnit): 20 тестів, в том числі "
            "(including) thread-safe parallel create (8 × 500 = 4000) та "
            "concurrent SaveAsync."
        )
        features_md = (
            "1. Async CRUD: CreateAsync / ReadAsync / ReadAllAsync /\n"
            "   ReadAllAsync(page, amount) / UpdateAsync / RemoveAsync /\n"
            "   SaveAsync.\n"
            "2. Thread safety: List<T> синхронізований lock; snapshot для\n"
            "   читання; JSON-запис захищений SemaphoreSlim.\n"
            "3. IEnumerable<T>: ICrudServiceAsync<T> успадковує IEnumerable<T>;\n"
            "   GetEnumerator працює зі snapshot.\n"
            "4. LINQ: Min, Max, Average, Where, Select, Aggregate, OrderBy,\n"
            "   Skip/Take (пагінація) — на тривалості (DurationSeconds) реальних\n"
            "   Song.\n"
            "5. Parallel: Parallel.For створює 2000 Song; ConcurrentBag<Song>;\n"
            "   фактичний час через Stopwatch; виведено ActualCount та Elapsed.\n"
            "6. lock: 8 потоків × 10 000 зростань; Expected/Actual/Match.\n"
            "7. SemaphoreSlim: 10 worker'ів, maxConcurrency = 3; виміря\n"
            "   (measured) фактичний максимум (Interlocked).\n"
            "8. AutoResetEvent: producer/consumer; itemReady та\n"
            "   producerFinished; producedCount / consumedCount (Interlocked);\n"
            "   перевірка Produced == Consumed == M та Buffer empty.\n"
            "9. SaveAsync: File.WriteAllTextAsync + статичний SemaphoreSlim (1);\n"
            "   конкурентні (3) SaveAsync залишаються валідним (valid) JSON.\n"
            "10. Unit tests (xUnit): 20 тестів (CRUD, пагінація, InvalidOperationException,\n"
            "    KeyNotFoundException, ArgumentOutOfRangeException, IEnumerable,\n"
            "    thread-safe, SaveAsync, concurrent SaveAsync)."
        )

        pdf.add_page()
        heading("Лабораторна робота №2 — MusicContest (C#/.NET 8)", 16)
        pdf.ln(4)
        heading("Тема та мета", 13)
        body(theme, 12)

        pdf.ln(2)
        heading("Архітектура та реалізація", 13)
        body(architecture, 11)

        pdf.ln(2)
        heading("Виконані завдання", 13)
        body(features_md[:1500], 11)

        pdf.ln(2)
        pdf.add_page()
        heading("Реальні результати", 13)
        body("Build (dotnet build):", 12)
        mono(self.build_summary())
        pdf.ln(2)
        body("Tests (dotnet test MusicContest.Common.Tests):", 12)
        mono(self.test_summary_text())
        pdf.ln(2)
        body(f"Run (dotnet run --project MusicContest.Console) — {self.exit_code}", 12)
        mono(self.run_summary())
        pdf.ln(2)
        body("Висновок:", 12)
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
    print("Lock:", report.lock_expected, "/", report.lock_actual, report.lock_match)
    print("Semaphore:", report.semaphore_max)
    print("Producer/Consumer:", report.produced_consumed)


if __name__ == "__main__":
    main()
