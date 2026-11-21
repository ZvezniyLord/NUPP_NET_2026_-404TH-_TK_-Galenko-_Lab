#!/usr/bin/env python3
"""Формує PDF-звіт ЛР2 з фактичних журналів збірки, тестування та запуску."""
from pathlib import Path
from html import escape

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    PageBreak, Preformatted
)

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
OUTPUT = DOCS / "Lab2_Report.pdf"

def read_required(name: str) -> str:
    path = DOCS / name
    if not path.exists():
        raise FileNotFoundError(f"Не знайдено журнал для звіту: {path}")
    return path.read_text(encoding="utf-8", errors="replace")

def register_fonts() -> tuple[str, str]:
    regular_candidates = [
        Path("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"),
        Path("C:/Windows/Fonts/arial.ttf"),
    ]
    bold_candidates = [
        Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"),
        Path("C:/Windows/Fonts/arialbd.ttf"),
    ]
    regular = next((p for p in regular_candidates if p.exists()), None)
    bold = next((p for p in bold_candidates if p.exists()), regular)
    if regular is None:
        raise FileNotFoundError("Не знайдено Unicode-шрифт DejaVu Sans або Arial.")
    pdfmetrics.registerFont(TTFont("ReportSans", str(regular)))
    pdfmetrics.registerFont(TTFont("ReportSans-Bold", str(bold)))
    return "ReportSans", "ReportSans-Bold"

def paragraph(text: str, style: ParagraphStyle) -> Paragraph:
    return Paragraph(escape(text).replace("\n", "<br/>"), style)

def main() -> None:
    build_log = read_required("Lab2_build.txt")
    test_log = read_required("Lab2_test.txt")
    output_log = read_required("Lab2_output.txt")
    regular, bold = register_fonts()

    styles = getSampleStyleSheet()
    styles.add(ParagraphStyle(
        name="ReportTitle", fontName=bold, fontSize=20, leading=25,
        alignment=TA_CENTER, textColor=colors.HexColor("#17324d"), spaceAfter=4*mm
    ))
    styles.add(ParagraphStyle(
        name="ReportSubtitle", fontName=regular, fontSize=10, leading=14,
        alignment=TA_CENTER, textColor=colors.HexColor("#526477"), spaceAfter=7*mm
    ))
    styles.add(ParagraphStyle(
        name="SectionUA", fontName=bold, fontSize=13, leading=17,
        textColor=colors.HexColor("#17324d"), spaceBefore=4*mm, spaceAfter=2*mm
    ))
    styles.add(ParagraphStyle(
        name="BodyUA", fontName=regular, fontSize=9.2, leading=13, spaceAfter=2*mm
    ))
    styles.add(ParagraphStyle(
        name="SmallUA", fontName=regular, fontSize=7.8, leading=10
    ))
    styles.add(ParagraphStyle(
        name="LogUA", fontName=regular, fontSize=7, leading=9,
        leftIndent=2*mm, rightIndent=2*mm
    ))

    story = [
        Spacer(1, 9*mm),
        Paragraph("ЛАБОРАТОРНА РОБОТА №2", styles["ReportTitle"]),
        Paragraph("MusicContest · Багатопотоковість, асинхронність, IEnumerable та LINQ",
                  styles["ReportSubtitle"]),
    ]

    meta = [
        [paragraph("Предметна область", styles["SmallUA"]),
         paragraph("Український вокальний конкурс", styles["SmallUA"])],
        [paragraph("Технології", styles["SmallUA"]),
         paragraph("C#, .NET 8, Task, Parallel, LINQ, JSON, xUnit", styles["SmallUA"])],
        [paragraph("Гілка", styles["SmallUA"]), paragraph("lab2", styles["SmallUA"])],
        [paragraph("Джерела результатів", styles["SmallUA"]),
         paragraph("Фактичні журнали dotnet build, dotnet test і dotnet run", styles["SmallUA"])],
    ]
    meta_table = Table(meta, colWidths=[45*mm, 125*mm])
    meta_table.setStyle(TableStyle([
        ("BACKGROUND",(0,0),(0,-1),colors.HexColor("#eaf0f5")),
        ("GRID",(0,0),(-1,-1),0.4,colors.HexColor("#cbd5df")),
        ("VALIGN",(0,0),(-1,-1),"MIDDLE"),
        ("LEFTPADDING",(0,0),(-1,-1),7),
        ("RIGHTPADDING",(0,0),(-1,-1),7),
        ("TOPPADDING",(0,0),(-1,-1),6),
        ("BOTTOMPADDING",(0,0),(-1,-1),6),
    ]))
    story += [
        meta_table,
        Paragraph("Мета роботи", styles["SectionUA"]),
        Paragraph(
            "Реалізувати асинхронний узагальнений CRUD-сервіс із потокобезпечним "
            "сховищем, пагінацією, підтримкою IEnumerable та асинхронним збереженням JSON. "
            "Продемонструвати паралельне створення об'єктів, LINQ-запити й засоби синхронізації.",
            styles["BodyUA"]
        ),
        Paragraph("Реалізовані складові", styles["SectionUA"])
    ]

    features = [
        ("Асинхронний CRUD", "CRUD-операції над ConcurrentDictionary виконуються синхронно; Task.Yield планує продовження. SaveAsync використовує File.WriteAllTextAsync."),
        ("Потокобезпечне сховище", "ConcurrentDictionary<Guid, T> та формування знімка колекції для читання й перебирання."),
        ("Паралельне створення", "Parallel.For створює 2000 об'єктів; завершення асинхронних операцій очікується через Task.WhenAll."),
        ("LINQ", "Min, Max, Average, Where, Select, Aggregate, OrderByDescending, Skip і Take."),
        ("Синхронізація", "lock, SemaphoreSlim, AutoResetEvent, Interlocked та демонстрація producer/consumer."),
        ("Збереження", "Асинхронний запис JSON із захистом від одночасного перезапису файла."),
        ("Модульні тести", "xUnit-тести для CRUD, пагінації, IEnumerable, паралельного доступу та збереження."),
        ("Демонстраційні дані", "Українські імена виконавців, назви колективів і пісні з набору UkrainianMusicData."),
    ]
    rows = [[paragraph("Складова", styles["SmallUA"]),
             paragraph("Реалізація", styles["SmallUA"])]]
    rows += [[paragraph(a, styles["SmallUA"]), paragraph(b, styles["SmallUA"])] for a,b in features]
    ft = Table(rows, colWidths=[43*mm, 127*mm], repeatRows=1)
    ft.setStyle(TableStyle([
        ("BACKGROUND",(0,0),(-1,0),colors.HexColor("#eaf0f5")),
        ("GRID",(0,0),(-1,-1),0.4,colors.HexColor("#cbd5df")),
        ("VALIGN",(0,0),(-1,-1),"TOP"),
        ("LEFTPADDING",(0,0),(-1,-1),6),
        ("RIGHTPADDING",(0,0),(-1,-1),6),
        ("TOPPADDING",(0,0),(-1,-1),5),
        ("BOTTOMPADDING",(0,0),(-1,-1),5),
    ]))
    story += [ft, Paragraph("Схема виконання", styles["SectionUA"])]

    flow = [[
        paragraph("Program.cs\nконсольний запуск", styles["SmallUA"]),
        paragraph("Parallell\nParallel.For", styles["SmallUA"]),
        paragraph("Song.CreateNew\nукраїнські дані", styles["SmallUA"]),
        paragraph("CrudServiceAsync\nConcurrentDictionary", styles["SmallUA"]),
        paragraph("LINQ / JSON\nрезультати", styles["SmallUA"]),
    ]]
    flow_table = Table(flow, colWidths=[34*mm]*5)
    flow_table.setStyle(TableStyle([
        ("BACKGROUND",(0,0),(-1,-1),colors.HexColor("#f3f6f9")),
        ("BOX",(0,0),(-1,-1),0.5,colors.HexColor("#aab8c5")),
        ("INNERGRID",(0,0),(-1,-1),0.5,colors.HexColor("#aab8c5")),
        ("VALIGN",(0,0),(-1,-1),"MIDDLE"),
        ("ALIGN",(0,0),(-1,-1),"CENTER"),
        ("TOPPADDING",(0,0),(-1,-1),8),
        ("BOTTOMPADDING",(0,0),(-1,-1),8),
    ]))
    story += [
        flow_table,
        PageBreak(),
        Paragraph("Результати збірки", styles["SectionUA"]),
        Preformatted("\n".join(build_log.splitlines()[-14:]), styles["LogUA"], maxLineLength=88, newLineChars="    "),
        Paragraph("Результати автоматизованих тестів", styles["SectionUA"]),
        Preformatted("\n".join(test_log.splitlines()[-18:]), styles["LogUA"], maxLineLength=88, newLineChars="    "),
        PageBreak(),
        Paragraph("Фактичний вивід консольного застосунку", styles["SectionUA"]),
        Paragraph(
            "Нижче наведено журнал, записаний під час запуску застосунку. "
            "Показники LINQ, паралельного створення та синхронізації отримано з виконання програми.",
            styles["BodyUA"]
        ),
        Preformatted(output_log, styles["LogUA"], maxLineLength=88, newLineChars="    "),
    ]

    def footer(canvas, doc):
        canvas.saveState()
        canvas.setFont(regular, 8)
        canvas.setFillColor(colors.HexColor("#667788"))
        canvas.drawString(18*mm, 10*mm, "MusicContest · Лабораторна робота №2")
        canvas.drawRightString(A4[0]-18*mm, 10*mm, f"Сторінка {doc.page}")
        canvas.restoreState()

    doc = SimpleDocTemplate(
        str(OUTPUT), pagesize=A4, rightMargin=18*mm, leftMargin=18*mm,
        topMargin=16*mm, bottomMargin=18*mm,
        title="Лабораторна робота №2: MusicContest", author="MusicContest"
    )
    doc.build(story, onFirstPage=footer, onLaterPages=footer)
    print(f"Створено {OUTPUT} ({OUTPUT.stat().st_size} байтів)")

if __name__ == "__main__":
    main()
