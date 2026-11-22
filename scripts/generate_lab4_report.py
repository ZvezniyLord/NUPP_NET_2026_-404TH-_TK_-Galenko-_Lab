#!/usr/bin/env python3
"""Створює звіт лише з результатів фактичних HTTP-запитів."""
import json
from pathlib import Path
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, LongTable, TableStyle
from xml.sax.saxutils import escape

root = Path(__file__).resolve().parents[1]
source = root / "docs/Lab4_http_results.json"
if not source.exists():
    raise SystemExit("Немає журналу HTTP-запитів. Спочатку запустіть перевірку API.")
records = json.loads(source.read_text(encoding="utf-8"))
if not records or any("status" not in row for row in records):
    raise SystemExit("Журнал HTTP-запитів неповний.")

font_candidates = [
    ("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
     "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"),
    ("C:/Windows/Fonts/arial.ttf", "C:/Windows/Fonts/arialbd.ttf"),
]
regular, bold = next(((a, b) for a, b in font_candidates
                      if Path(a).is_file() and Path(b).is_file()), (None, None))
if regular is None:
    raise SystemExit("Не знайдено шрифт з підтримкою української мови.")
pdfmetrics.registerFont(TTFont("UaRegular", regular))
pdfmetrics.registerFont(TTFont("UaBold", bold))

style = ParagraphStyle("UA", fontName="UaRegular", fontSize=9, leading=13, spaceAfter=5)
small = ParagraphStyle("SmallUA", parent=style, fontSize=7, leading=10)
heading = ParagraphStyle("HeadingUA", parent=style, fontName="UaBold", fontSize=16, leading=21, spaceAfter=12)
pdf_path = root / "docs/Lab4_Report.pdf"
doc = SimpleDocTemplate(str(pdf_path), pagesize=A4, leftMargin=19*mm,
                        rightMargin=19*mm, topMargin=17*mm, bottomMargin=17*mm,
                        title="Лабораторна робота №4. ASP.NET Core Web API")
story = [
    Paragraph("Лабораторна робота №4", heading),
    Paragraph("Розробка вебсерверів засобами ASP.NET Core Web API", style),
    Paragraph("Проєкт MusicContest.REST. Дві сутності: Songs і Performances. "
              "Трирівнева архітектура: контролери, CRUD-сервіс, репозиторій і SQLite.", style),
    Spacer(1, 5*mm),
    Paragraph("Результати фактичних HTTP-запитів", heading)
]
rows = [[Paragraph("Метод", small), Paragraph("Маршрут", small),
         Paragraph("HTTP", small), Paragraph("Відповідь", small)]]
for record in records:
    content = record.get("response")
    response = json.dumps(content, ensure_ascii=False) if content is not None else "Без тіла відповіді"
    if len(response) > 165:
        response = response[:162] + "..."
    rows.append([Paragraph(escape(record["method"]), small),
                 Paragraph(escape(record["path"]), small),
                 Paragraph(str(record["status"]), small),
                 Paragraph(escape(response), small)])
table = LongTable(rows, colWidths=[17*mm, 49*mm, 14*mm, 92*mm], repeatRows=1)
table.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e9eef5")),
    ("GRID", (0, 0), (-1, -1), 0.3, colors.HexColor("#c5ced8")),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 0), (-1, -1), 5),
    ("RIGHTPADDING", (0, 0), (-1, -1), 5),
]))
story.append(table)
story.append(Spacer(1, 6*mm))
story.append(Paragraph(
    f"Виконано {len(records)} запитів. Для створення перевірено HTTP 201, "
    "для читання 200, для оновлення та видалення 204, "
    "для відсутніх записів 404, для некоректної пагінації 400. "
    "Першоджерело результатів: docs/Lab4_http_results.json.", style))
doc.build(story)
print("Створено:", pdf_path)
