#!/usr/bin/env python3
"""Сумісний запуск актуального генератора звітів ЛР1 та ЛР2.

Основна реалізація міститься у scripts/generate_lab12_reports.py.
Дані беруться з фактичних журналів запуску, збірки й тестів.
"""
from pathlib import Path
import runpy

generator = Path(__file__).resolve().parent / "scripts" / "generate_lab12_reports.py"
runpy.run_path(str(generator), run_name="__main__")
