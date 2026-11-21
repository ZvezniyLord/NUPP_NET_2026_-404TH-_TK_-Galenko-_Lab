#!/usr/bin/env python3
"""Створення PDF-звіту ЛР1 на основі фактичних журналів .NET."""
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
    PageBreak, Preformatted, KeepTogether
)

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
OUTPUT = DOCS / "Lab1_Report.pdf"

def required(name: str) -> str:
    path = DOCS / name
    if not path.exists():
        raise FileNotFoundError(f"Не знайдено обов'язковий журнал: {path}")
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

def main() -> None:
    build_log = required("Lab1_build.txt")
    output_log = required("Lab1_output.txt")
    regular, bold = register_fonts()

    styles = getSampleStyleSheet()
    styles.add(ParagraphStyle(
        name="ReportTitle", fontName=bold, fontSize=21, leading=26,
        alignment=TA_CENTER, textColor=colors.HexColor("#17324d"), spaceAfter=5*mm
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
        name="BodyUA", fontName=regular, fontSize=9.5, leading=14, spaceAfter=2*mm
    ))
    styles.add(ParagraphStyle(
        name="SmallUA", fontName=regular, fontSize=8, leading=11
    ))
    styles.add(ParagraphStyle(
        name="LogUA", fontName=regular, fontSize=7.1, leading=9,
        leftIndent=2*mm, rightIndent=2*mm
    ))

    story = [
        Spacer(1, 12*mm),
        Paragraph("ЛАБОРАТОРНА РОБОТА №1", styles["ReportTitle"]),
        Paragraph("MusicContest · C# / .NET · Основи ООП та CRUD", styles["ReportSubtitle"]),
    ]

    meta = [
        [Paragraph("<b>Предметна область</b>", styles["SmallUA"]),
         Paragraph("Український вокальний конкурс", styles["SmallUA"])],
        [Paragraph("<b>Основні технології</b>", styles["SmallUA"]),
         Paragraph("C#, .NET, JSON, generic types, events, delegates", styles["SmallUA"])],
        [Paragraph("<b>Гілка</b>", styles["SmallUA"]),
         Paragraph("lab1", styles["SmallUA"])],
        [Paragraph("<b>Перевірка</b>", styles["SmallUA"]),
         Paragraph("Збірка та запуск консольного застосунку", styles["SmallUA"])],
    ]
    table = Table(meta, colWidths=[48*mm, 122*mm])
    table.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (0,-1), colors.HexColor("#eaf0f5")),
        ("BACKGROUND", (1,0), (1,-1), colors.white),
        ("GRID", (0,0), (-1,-1), 0.4, colors.HexColor("#cbd5df")),
        ("VALIGN", (0,0), (-1,-1), "MIDDLE"),
        ("LEFTPADDING", (0,0), (-1,-1), 7),
        ("RIGHTPADDING", (0,0), (-1,-1), 7),
        ("TOPPADDING", (0,0), (-1,-1), 6),
        ("BOTTOMPADDING", (0,0), (-1,-1), 6),
    ]))
    story += [table, Paragraph("Мета роботи", styles["SectionUA"]),
        Paragraph(
            "Закріпити основи об'єктно-орієнтованого програмування в C#: "
            "створення класів та об'єктів, наслідування, поліморфізм, "
            "статичні члени, делегати, події, методи розширення та узагальнений CRUD-сервіс. "
            "Додатково реалізувати збереження й відновлення даних у форматі JSON.",
            styles["BodyUA"]
        ),
        Paragraph("Реалізовані складові", styles["SectionUA"])
    ]

    features = [
        ("Модель предметної області", "Абстрактний клас ContestParticipant та похідні класи SoloSinger і VocalGroup; моделі Song та Performance."),
        ("Основи ООП", "Конструктори, властивості, наслідування, перевизначення методів і поліморфізм."),
        ("Статичні члени", "Статичне поле, статичний конструктор і статичний метод для підрахунку створених об'єктів."),
        ("Події та делегати", "Делегат ScoreChangedHandler і подія ScoreChanged для відстеження зміни оцінки виступу."),
        ("Generic CRUD", "Операції Create, Read, ReadAll, Update та Remove для типів, що реалізують IEntity."),
        ("JSON", "Методи Save та Load для серіалізації й десеріалізації даних."),
    ]
    rows = [[Paragraph("<b>Складова</b>", styles["SmallUA"]),
             Paragraph("<b>Реалізація</b>", styles["SmallUA"])]]
    rows += [[Paragraph(escape(a), styles["SmallUA"]),
              Paragraph(escape(b), styles["SmallUA"])] for a,b in features]
    ft = Table(rows, colWidths=[43*mm, 127*mm], repeatRows=1)
    ft.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,0), colors.HexColor("#eaf0f5")),
        ("GRID", (0,0), (-1,-1), 0.4, colors.HexColor("#cbd5df")),
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("LEFTPADDING", (0,0), (-1,-1), 6),
        ("RIGHTPADDING", (0,0), (-1,-1), 6),
        ("TOPPADDING", (0,0), (-1,-1), 5),
        ("BOTTOMPADDING", (0,0), (-1,-1), 5),
    ]))
    story += [ft, Paragraph("Схема виконання", styles["SectionUA"])]

    flow = [[
        Paragraph("<b>Console</b><br/>демонстрація", styles["SmallUA"]),
        Paragraph("<b>Моделі</b><br/>учасники, пісні, виступи", styles["SmallUA"]),
        Paragraph("<b>CrudService&lt;T&gt;</b><br/>операції CRUD", styles["SmallUA"]),
        Paragraph("<b>List&lt;T&gt;</b><br/>колекція", styles["SmallUA"]),
        Paragraph("<b>JSON</b><br/>Save / Load", styles["SmallUA"]),
    ]]
    ft2 = Table(flow, colWidths=[34*mm,34*mm,34*mm,34*mm,34*mm])
    ft2.setStyle(TableStyle([
        ("BACKGROUND",(0,0),(-1,-1),colors.HexColor("#f3f6f9")),
        ("BOX",(0,0),(-1,-1),0.5,colors.HexColor("#aab8c5")),
        ("INNERGRID",(0,0),(-1,-1),0.5,colors.HexColor("#aab8c5")),
        ("VALIGN",(0,0),(-1,-1),"MIDDLE"),
        ("ALIGN",(0,0),(-1,-1),"CENTER"),
        ("TOPPADDING",(0,0),(-1,-1),8),
        ("BOTTOMPADDING",(0,0),(-1,-1),8),
    ]))
    story += [ft2, PageBreak(),
        Paragraph("Результати збірки", styles["SectionUA"]),
        Preformatted("\n".join(build_log.splitlines()[-14:]), styles["LogUA"]),
        Paragraph("Фактичний вивід консольного застосунку", styles["SectionUA"]),
        Paragraph(
            "Нижче наведено журнал запуску, сформований безпосередньо під час виконання програми.",
            styles["BodyUA"]
        ),
        Preformatted(output_log, styles["LogUA"])
    ]

    def footer(canvas, doc):
        canvas.saveState()
        canvas.setFont(regular, 8)
        canvas.setFillColor(colors.HexColor("#667788"))
        canvas.drawString(18*mm, 10*mm, "MusicContest · Лабораторна робота №1")
        canvas.drawRightString(A4[0]-18*mm, 10*mm, f"Сторінка {doc.page}")
        canvas.restoreState()

    doc = SimpleDocTemplate(
        str(OUTPUT), pagesize=A4, rightMargin=18*mm, leftMargin=18*mm,
        topMargin=16*mm, bottomMargin=18*mm, title="Лабораторна робота №1: MusicContest",
        author="MusicContest"
    )
    doc.build(story, onFirstPage=footer, onLaterPages=footer)
    print(f"Створено {OUTPUT} ({OUTPUT.stat().st_size} байтів)")

if __name__ == "__main__":
    main()
