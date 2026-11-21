#!/usr/bin/env python3
"""Сумісна команда для запуску основного генератора PDF-звіту ЛР2."""
from pathlib import Path
import runpy

generator = Path(__file__).resolve().parent / "docs" / "Lab2_report_generator.py"
runpy.run_path(str(generator), run_name="__main__")
