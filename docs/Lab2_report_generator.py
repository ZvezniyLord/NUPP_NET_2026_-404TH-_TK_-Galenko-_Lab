#!/usr/bin/env python3
"""Сумісний запуск генератора звітів ЛР1 та ЛР2.

PDF-звіти формує scripts/generate_lab12_reports.py з фактичних
журналів без дублювання логіки оформлення.
"""
from pathlib import Path
import runpy

generator = Path(__file__).resolve().parents[1] / "scripts" / "generate_lab12_reports.py"
runpy.run_path(str(generator), run_name="__main__")
