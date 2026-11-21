#!/usr/bin/env python3
"""Візуалізація реальних документів MongoDB з лабораторної №3."""
import json
from pathlib import Path
from pymongo import MongoClient
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "Lab3_MongoDB.png"
FONT_FILE = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
BOLD_FILE = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
font = lambda size, bold=False: ImageFont.truetype(BOLD_FILE if bold else FONT_FILE, size)

client = MongoClient("mongodb://127.0.0.1:27017/", serverSelectionTimeoutMS=10000)
client.admin.command("ping")
docs = list(client["MusicContestLab3"]["songs"].find({}).sort("DatabaseId", 1))
if not docs:
    raise RuntimeError("MongoDB collection is empty, cannot create evidence image")

im = Image.new("RGB", (1600, 950), "#ffffff")
draw = ImageDraw.Draw(im)
draw.rectangle((0,0,1600,86),fill="#153148")
draw.text((32,20),"MongoDB — документи MusicContestLab3.songs",font=font(31,True),fill="white")
draw.text((36,108),"Джерело: реальна колекція MongoDB; запит find({}).sort('DatabaseId',1)",font=font(20),fill="#40546c")

y=160
for doc in docs[:3]:
    doc["_id"] = str(doc["_id"])
    as_json = json.dumps(doc, ensure_ascii=False, indent=2, default=str)
    draw.rounded_rectangle((30,y,1570,y+325),radius=14,fill="#f1f6fb",outline="#a8bdd1",width=2)
    yy=y+18
    for line in as_json.splitlines():
        draw.text((55,yy),line,font=font(20),fill="#203951")
        yy+=29
    y+=350

OUT.parent.mkdir(exist_ok=True)
im.save(OUT)
print(f"MongoDB screenshot visualization: {OUT}, documents: {len(docs)}")
