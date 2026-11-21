#!/usr/bin/env python3
"""Формує PDF ЛР1 і ЛР2 виключно з фактичних консольних логів.

Немає заглушок і синтетичних скриншотів: тільки перевірений вивід
команд dotnet run / dotnet build / dotnet test у GitHub Actions.
"""
from pathlib import Path
import textwrap
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
FONT = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf")
FONT_B = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf")

def read_required(name: str) -> list[str]:
    path = DOCS / name
    if not path.exists():
        raise FileNotFoundError(f"Відсутній доказовий файл: {path}")
    lines = path.read_text(encoding="utf-8").splitlines()
    if not lines:
        raise ValueError(f"Порожній доказовий файл: {path}")
    return lines

def render_pdf(number: int, goal: str, explanations: list[str], output: list[str],
               build: list[str], tests: list[str] | None) -> Path:
    out = DOCS / f"Lab{number}_Report.pdf"
    pdfmetrics.registerFont(TTFont("DejaVu", str(FONT)))
    pdfmetrics.registerFont(TTFont("DejaVuBold", str(FONT_B)))
    c = canvas.Canvas(str(out), pagesize=A4)
    pw, ph = A4
    top = ph - 62
    page_num = 0

    def new_page(title):
        nonlocal top, page_num
        if page_num:
            c.showPage()
        page_num += 1
        top = ph - 58
        c.setFont("DejaVuBold", 16)
        c.drawString(42, top, title)
        top -= 31

    def line(text, size=9.3, bold=False, indent=0):
        nonlocal top
        font_name = "DejaVuBold" if bold else "DejaVu"
        max_chars = max(40, 104 - indent // 2)
        for frag in textwrap.wrap(str(text), max_chars, break_long_words=True,
                                  break_on_hyphens=False) or [""]:
            if top < 58:
                new_page("Лабораторна робота №%s: продовження" % number)
            c.setFont(font_name, size)
            c.drawString(42 + indent, top, frag)
            top -= 16

    new_page(f"Лабораторна робота №{number}: MusicContest")
    line(f"Мета: {goal}", 10)
    top -= 8
    line("Предметна область: український вокальний конкурс.", 10)
    line("Навчальні імена конкурсантів та назви гуртів є вигаданими.", 9)
    top -= 15

    line("Що реалізовано", 12, True)
    for item in explanations:
        line("• " + item, 9.2, indent=8)
    top -= 16

    line("Результати збірки", 12, True)
    line("dotnet build", 10, True, 8)
    for item in build[-7:]:
        line(item, 8, indent=16)
    top -= 8

    if tests is not None:
        line("Результати автоматизованих тестів", 12, True)
        line("dotnet test", 10, True, 8)
        for item in tests[-7:]:
            line(item, 8, indent=16)
        top -= 8

    new_page(f"ЛР{number}. Фактичний вивід консольного застосунку")
    line("Дані нижче перенесено без вигаданих результатів із docs/Lab%d_output.txt" % number, 8)
    top -= 14
    for row in output:
        line(row, 8, indent=4)

    c.setTitle(f"Лабораторна робота №{number}: MusicContest")
    c.setAuthor("MusicContest: навчальний проєкт")
    c.save()
    print(f"Сформовано {out} із реальних логів; сторінок: {page_num}")
    return out

def main():
    build_logs = {
        1: read_required("Lab1_build.txt"),
        2: read_required("Lab2_build.txt"),
    }
    test_logs = {
        1: None,
        2: read_required("Lab2_test.txt"),
    }
    for number, goal, features in (
        (1, "моделювання класів C#, ООП, generic CRUD, делегатів, подій та JSON",
         ["Абстрактний клас ContestParticipant і наслідування SoloSinger/VocalGroup.",
          "Інкапсуляція, статичні члени, перевизначення GetDescription().",
          "Делегат та подія Performance.ScoreChanged.",
          "Generic CRUD (Create, Read, Update, Remove) та JSON Save/Load.",
          "Реальні українські назви навчальних учасників і пісень."]),
        (2, "асинхронний CRUD, багатопотоковість, IEnumerable та LINQ",
         ["ICrudServiceAsync<T>, ConcurrentDictionary<Guid,T> та snapshot.",
          "Parallel.For створює 2000 об'єктів, Task.WhenAll завершує додавання.",
          "LINQ: Min, Max, Average, Where, Select, Aggregate, OrderBy.",
          "Пагінація, foreach, асинхронна JSON-серіалізація.",
          "Синхронізація: lock, SemaphoreSlim, AutoResetEvent.",
          "Репертуар і виконавці генеруються з українського набору."])
    ):
        lines = read_required(f"Lab{number}_output.txt")
        if number == 2:
            header = next((i for i, item in enumerate(lines)
                           if item.strip() == "ЛАБОРАТОРНА РОБОТА №2"), -1)
            if header < 0:
                raise ValueError("У реальному виході немає блоку ЛР2")
            lines = lines[max(0, header-1):]
        render_pdf(number, goal, features, lines, build_logs[number], test_logs[number])

if __name__ == "__main__":
    main()
