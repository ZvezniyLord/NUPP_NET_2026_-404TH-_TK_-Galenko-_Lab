#!/usr/bin/env python3
"""Перевірка текстового шару PDF-звітів і створення зображень для огляду.

Перевіряє фактично згенерований PDF, не підміняє його вміст шаблоном.
Потрібні утиліти poppler-utils: pdfinfo, pdftotext та pdftoppm.
"""
from __future__ import annotations

from pathlib import Path
import re
import subprocess
import sys


def run(*command: str) -> str:
    result = subprocess.run(command, text=True, capture_output=True, check=True)
    return result.stdout


def check_pdf(path: Path) -> None:
    if not path.is_file():
        raise FileNotFoundError(f"Не знайдено звіт: {path}")
    information = run("pdfinfo", str(path))
    match = re.search(r"^Pages:\s*(\d+)\s*$", information, re.MULTILINE)
    if match is None or int(match.group(1)) < 1:
        raise ValueError(f"PDF не містить сторінок: {path}")

    text_path = path.with_name(path.stem + "_extracted.txt")
    run("pdftotext", "-layout", str(path), str(text_path))
    text = text_path.read_text(encoding="utf-8")

    if len(text.strip()) < 200 or not re.search(r"[іїєґІЇЄҐ]", text):
        raise ValueError(f"PDF має недостатній український текстовий шар: {path}")

    # Лише очевидні залишки іншомовного тексту, без автоматичного
    # виправлення власних назв та професійної термінології.
    forbidden = re.search(
        r"[ыЫэЭъЪёЁ]|\b(?:проверяємо|валидація|проверка|выполнено|добавить|исходный)\b",
        text, re.IGNORECASE
    )
    if forbidden:
        raise ValueError(
            f"Підозрілий мовний фрагмент у {path.name}: {forbidden.group(0)!r}"
        )

    preview_base = path.with_name(path.stem + "_preview")
    run("pdftoppm", "-f", "1", "-l", "1", "-singlefile",
        "-scale-to", "1200", "-png", str(path), str(preview_base))

    if path.stem == "Lab3_Report" and int(match.group(1)) >= 4:
        erd_base = path.with_name(path.stem + "_preview_erd")
        run("pdftoppm", "-f", "4", "-l", "4", "-singlefile",
            "-scale-to", "1200", "-png", str(path), str(erd_base))

    print(
        f"Перевірено {path.name}: {match.group(1)} сторінок, "
        f"{len(text)} символів тексту; створено прев'ю."
    )


if __name__ == "__main__":
    if len(sys.argv) < 2:
        raise SystemExit("Використання: python scripts/check_pdf_language.py docs/LabX_Report.pdf")
    for file_name in sys.argv[1:]:
        check_pdf(Path(file_name))
