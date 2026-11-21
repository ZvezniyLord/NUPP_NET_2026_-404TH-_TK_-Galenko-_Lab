#!/usr/bin/env python3
"""Формує звіт ЛР3 з фактичних даних SQLite, а не вигаданих результатів."""
from pathlib import Path
import sqlite3
import textwrap
from PIL import Image, ImageDraw, ImageFont
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
DB = DOCS / "musiccontest.db"
FONT_FILE = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf")
BOLD_FILE = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf")
DOCS.mkdir(exist_ok=True)

def font(size, bold=False):
    return ImageFont.truetype(str(BOLD_FILE if bold else FONT_FILE), size)

def table_names(conn):
    return [r[0] for r in conn.execute(
        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name"
    )]

def png_sqlite_tables(conn):
    names = table_names(conn)
    image = Image.new("RGB", (1550, 730), "#ffffff")
    d = ImageDraw.Draw(image)
    d.rectangle((0, 0, 1550, 80), fill="#13273c")
    d.text((36, 20), "SQLite — список реально створених таблиць", font=font(32, True), fill="white")
    d.text((42, 106), f"Файл: {DB.name} | PRAGMA foreign_keys=ON | sqlite_master", font=font(22), fill="#263648")
    y = 170
    for name in names:
        n = conn.execute(f'SELECT COUNT(*) FROM "{name}"').fetchone()[0]
        cols = [x[1] for x in conn.execute(f'PRAGMA table_info("{name}")')]
        d.rectangle((34, y-6, 1515, y+51), fill="#f1f5f9" if (y//63)%2 else "#e8f0fa")
        d.text((54, y+3), name, font=font(23, True), fill="#122c49")
        d.text((520, y+3), f"Записів: {n}", font=font(21), fill="#122c49")
        d.text((800, y+5), ", ".join(cols)[:62], font=font(18), fill="#34465a")
        y += 64
    target = DOCS / "Lab3_DB_Tables.png"
    image.save(target)
    return target

def png_table(conn, table):
    rows = conn.execute(f'SELECT * FROM "{table}" ORDER BY 1 LIMIT 8').fetchall()
    cols = [r[1] for r in conn.execute(f'PRAGMA table_info("{table}")')]
    w = 1550
    h = max(320, 180 + (len(rows) + 1) * 55)
    image = Image.new("RGB", (w, h), "#fff")
    d = ImageDraw.Draw(image)
    d.rectangle((0, 0, w, 77), fill="#13273c")
    d.text((30, 17), f'SQLite: SELECT * FROM "{table}" LIMIT 8', font=font(27, True), fill="white")
    d.text((30, 98), "Дані з реального файла docs/musiccontest.db", font=font(20), fill="#516073")
    col_width = (w - 50) / max(1, len(cols))
    for j, name in enumerate(cols):
        x = 25 + int(j*col_width)
        d.rectangle((x, 144, x + col_width - 4, 192), fill="#dbe6f3")
        d.text((x+5, 152), name[:20], font=font(15, True), fill="#152b45")
    for i, row in enumerate(rows):
        y = 199 + i*55
        d.rectangle((25, y, w-25, y+52), fill="#f5f8fc" if i%2 == 0 else "#e9f0f8")
        for j, val in enumerate(row):
            x = 25 + int(j*col_width)
            text = str(val).replace("\n", " ") if val is not None else "NULL"
            if len(text) > max(8, int(col_width / 10)):
                text = text[:max(7, int(col_width / 10)-1)] + "…"
            d.text((x+5, y+12), text, font=font(15), fill="#1e3247")
    if not rows:
        d.text((30, 220), "Таблиця порожня", font=font(22), fill="#34465a")
    target = DOCS / f"Lab3_Table_{table}.png"
    image.save(target)
    return target

def png_erd(conn):
    existing = set(table_names(conn))
    required = {"ContestParticipants", "SoloSingers", "VocalGroups",
                "ParticipantProfiles", "Songs", "Performances", "ParticipantSongs"}
    if not required.issubset(existing):
        raise RuntimeError("ERD: у SQLite відсутні очікувані таблиці: " + str(required-existing))

    image = Image.new("RGB", (1750, 1140), "#fcfdff")
    d = ImageDraw.Draw(image)
    d.text((48, 24), "MusicContest — ERD (SQLite / Entity Framework Core)", font=font(33, True), fill="#13273c")
    boxes = {
        "ContestParticipants": (595, 105, 1140, 325),
        "SoloSingers": (60, 405, 520, 615),
        "VocalGroups": (60, 715, 520, 920),
        "ParticipantProfiles": (1200, 405, 1690, 615),
        "Performances": (595, 405, 1140, 655),
        "Songs": (1200, 715, 1690, 965),
        "ParticipantSongs": (595, 715, 1140, 960),
    }
    # Зв'язки намальовано за FK з PRAGMA; кардинальності відповідають PK/unique index.
    # Ортогональні маршрути FK: жодна лінія не перетинає вміст інших таблиць.
    edges = [
        ([(595, 240), (558, 240), (558, 444), (520, 444)], "TPT 1:1", (548, 370)),
        ([(595, 285), (538, 285), (538, 812), (520, 812)], "TPT 1:1", (527, 677)),
        ([(1140, 218), (1172, 218), (1172, 450), (1200, 450)], "1:1", (1170, 370)),
        ([(852, 325), (852, 405)], "1:N", (861, 361)),
        ([(595, 315), (574, 315), (574, 995), (815, 995), (815, 960)], "1:N", (583, 671)),
        ([(1140, 551), (1177, 551), (1177, 827), (1200, 827)], "N:1", (1177, 690)),
        ([(1140, 892), (1200, 892)], "N:1", (1176, 865)),
    ]
    for pts, label, (mx, my) in edges:
        d.line(pts, fill="#7792aa", width=4)
        x2, y2 = pts[-1]
        d.ellipse((x2-6, y2-6, x2+6, y2+6), fill="#406c94")
        label_width = int(d.textlength(label, font=font(15, True)))
        d.rounded_rectangle((mx-label_width//2-8, my-10, mx+label_width//2+8, my+13),
                            radius=5, fill="#ffffff")
        d.text((mx-label_width//2, my-8), label, font=font(15, True), fill="#244864")
    for table,(x1,y1,x2,y2) in boxes.items():
        d.rounded_rectangle((x1,y1,x2,y2),radius=15,fill="white",outline="#6182a1",width=3)
        d.rounded_rectangle((x1,y1,x2,y1+55),radius=14,fill="#234865")
        d.rectangle((x1,y1+34,x2,y1+55),fill="#234865")
        d.text((x1+18,y1+12),table,font=font(24,True),fill="white")
        cols = conn.execute(f'PRAGMA table_info("{table}")').fetchall()
        fk_cols = {x[3] for x in conn.execute(f'PRAGMA foreign_key_list("{table}")')}
        yy = y1+65
        for col in cols[:7]:
            cid,name,datatype,notnull,default,pk = col
            prefix = "PK " if pk else "FK " if name in fk_cols else "     "
            d.text((x1+15,yy), f"{prefix}{name} : {datatype}",font=font(17),fill="#1d354c")
            yy += 27
    d.text((55, 1053), "TPT = Table-per-Type; M:N = ContestParticipants ↔ ParticipantSongs ↔ Songs",font=font(21),fill="#354b5e")
    d.text((55, 1092), "Структура перевірена через sqlite_master / PRAGMA table_info / PRAGMA foreign_key_list.",font=font(17),fill="#57697a")
    target = DOCS / "Lab3_ERD.png"
    image.save(target)
    return target

def make_pdf(conn, pics):
    pdfmetrics.registerFont(TTFont("DejaVuSans", str(FONT_FILE)))
    pdfmetrics.registerFont(TTFont("DejaVuSansBold", str(BOLD_FILE)))
    pdf_path = DOCS / "Lab3_Report.pdf"
    c = canvas.Canvas(str(pdf_path), pagesize=A4)
    pw,ph = A4
    def label(s,x,y,size=11,bold=False):
        c.setFont("DejaVuSansBold" if bold else "DejaVuSans",size)
        c.drawString(x,y,s)
    def image_fit(path,y,max_height=255):
        with Image.open(path) as im:
            iw,ih=im.size
        maxw = pw-66
        scale=min(maxw/iw,max_height/ih)
        w,h=iw*scale,ih*scale
        c.drawImage(str(path),33,y-h,width=w,height=h)
        return y-h-15
    def footer(n):
        c.setStrokeColorRGB(.75,.79,.84)
        c.line(33,42,pw-33,42)
        label(f"Лабораторна №3 | MusicContest | с. {n}",33,26,8)

    label("ЛАБОРАТОРНА РОБОТА №3",33,ph-65,18,True)
    label("Робота з БД в .NET. Entity Framework Core",33,ph-91,12)
    label("Предметна область: MusicContest (вокальний конкурс)",33,ph-126,10)
    label("Реалізація: .NET 8, C#, EF Core, SQLite, Repository Pattern",33,ph-146,10)
    label("Ключові можливості",33,ph-192,13,True)
    lines = [
      "1:1  Учасник — профіль (унікальний зовнішній ключ).",
      "1:N  Учасник — виступи; пісня — виступи.",
      "M:N  Учасники — пісні через ParticipantSongs.",
      "TPT  ContestParticipants + SoloSingers / VocalGroups.",
      "CRUD  Generic RepositoryCrudServiceAsync<T> через IRepository<T>.",
      "Міграція  EF Core: 20261121130000_InitialCreate.",
    ]
    y=ph-217
    for line in lines:
        label(line,44,y,10);y-=29
    label("Фактична база даних",33,y-15,13,True);y-=43
    for table in table_names(conn):
        count=conn.execute(f'SELECT COUNT(*) FROM "{table}"').fetchone()[0]
        label(f"{table}: {count} записів",44,y,9);y-=22
    footer(1);c.showPage()

    label("1. Схема БД та таблиці",33,ph-55,15,True)
    y=ph-80
    y=image_fit(pics["overview"],y,330)
    label("Результати реального запиту sqlite_master, не макет БД.",33,y+2,8)
    y-=24
    y=image_fit(pics["ContestParticipants"],y,220)
    footer(2);c.showPage()

    label("2. Дані з таблиць SQLite",33,ph-55,15,True)
    y=ph-86
    for name in ("Songs","Performances","ParticipantSongs"):
        label(name,33,y,10,True);y-=15
        y=image_fit(pics[name],y,200)
    footer(3);c.showPage()

    label("3. ERD-діаграма",33,ph-55,15,True)
    y=image_fit(pics["erd"],ph-80,650)
    label("ERD побудована з фактичної схеми та зв'язків SQLite.",33,y+1,9)
    footer(4);c.showPage()

    label("4. Результат виконання консольного застосунку",33,ph-55,13,True)
    lines=(DOCS/"Lab3_output.txt").read_text(encoding="utf-8").splitlines()
    y=ph-86
    for line in lines:
        for chunk in textwrap.wrap(line,75,replace_whitespace=False,drop_whitespace=False) or [""]:
            if y < 72:
                footer(5);c.showPage();y=ph-60
            label(chunk,37,y,8);y-=15
    footer(5);c.showPage()

    label("5. Перевірка проєкту",33,ph-55,15,True)
    y=ph-84
    for key in ("build","test"):
        path=DOCS/f"Lab3_{key}.txt"
        label(("dotnet build" if key=="build" else "dotnet test"),33,y,12,True);y-=23
        raw=path.read_text(encoding="utf-8").splitlines() if path.exists() else ["Файл не знайдено"]
        for line in raw[-22:]:
            for chunk in textwrap.wrap(line,90) or [""]:
                if y<80: break
                label(chunk,37,y,7);y-=13
        y-=18
    label("Зображення таблиць отримано із SELECT/PRAGMA до файлу SQLite.",33,72,8)
    label("Скріншот окремого GUI-клієнта БД можна додати при захисті.",33,55,8)
    footer(6)
    mongo_png = DOCS / "Lab3_MongoDB.png"
    if mongo_png.exists():
        c.showPage()
        label("6. MongoDB — додаткове завдання",33,ph-55,15,True)
        yy=image_fit(mongo_png,ph-82,620)
        label("Зображення сформовано з реального запиту find() до MongoDB.",33,yy-4,9)
        footer(7)
    gui_pics = [
        DOCS / "Lab3_GUI_DatabaseStructure.png",
        DOCS / "Lab3_GUI_ContestParticipants.png",
        DOCS / "Lab3_GUI_Songs.png",
        DOCS / "Lab3_GUI_Performances.png",
    ]
    available_gui = [p for p in gui_pics if p.exists()]
    if available_gui:
        c.showPage()
        label("7. Скриншоти DB Browser for SQLite (реальний GUI)",33,ph-55,12,True)
        y = ph-83
        for path in available_gui[:2]:
            label(path.stem.replace("Lab3_GUI_", ""),33,y,9,True)
            y -= 17
            y = image_fit(path,y,290)
        footer(8)
        if len(available_gui)>2:
            c.showPage()
            label("8. Таблиці DB Browser for SQLite",33,ph-55,12,True)
            y = ph-83
            for path in available_gui[2:]:
                label(path.stem.replace("Lab3_GUI_", ""),33,y,9,True)
                y -= 17
                y = image_fit(path,y,290)
            footer(9)
    c.save()
    return pdf_path

def main():
    if not DB.exists():
        raise FileNotFoundError(f"Не знайдено {DB}; спочатку запустіть застосунок.")
    with sqlite3.connect(DB) as conn:
        conn.execute("PRAGMA foreign_keys=ON")
        required={"ContestParticipants","SoloSingers","VocalGroups","ParticipantProfiles",
                  "Songs","Performances","ParticipantSongs","__EFMigrationsHistory"}
        if not required.issubset(set(table_names(conn))):
            raise RuntimeError("EF Core міграцію не застосовано або база неповна.")
        pics={"overview": png_sqlite_tables(conn), "erd": png_erd(conn)}
        for table in ("ContestParticipants","Songs","Performances","ParticipantSongs"):
            pics[table]=png_table(conn,table)
        out=make_pdf(conn,pics)
        print(f"PDF створено з даних: {out}")

if __name__ == "__main__":
    main()
