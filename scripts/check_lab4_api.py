#!/usr/bin/env python3
"""Перевіряє HTTP-відповіді API та записує фактичні результати."""
import json
import os
import urllib.error
import urllib.request
from pathlib import Path

BASE = os.environ.get("LAB4_BASE_URL", "http://127.0.0.1:5084")
results = []

def request(method, path, payload=None):
    body = json.dumps(payload, ensure_ascii=False).encode("utf-8") if payload is not None else None
    req = urllib.request.Request(BASE + path, data=body, method=method,
                                 headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=15) as response:
            status = response.status
            content = response.read().decode("utf-8")
            location = response.headers.get("Location")
    except urllib.error.HTTPError as error:
        status = error.code
        content = error.read().decode("utf-8")
        location = None
    response_data = None
    if content:
        try:
            response_data = json.loads(content)
        except json.JSONDecodeError:
            response_data = content[:400]
    results.append({"method": method, "path": path, "status": status, "response": response_data, "location": location})
    return status, response_data

def expect(status, actual, description):
    if actual != status:
        raise AssertionError(f"{description}: очікувався HTTP {status}, отримано {actual}")

song = {"title": "Тестова українська пісня", "originalArtist": "Навчальний конкурс",
        "durationSeconds": 184, "key": "до мажор"}
expect(200, request("GET", "/api/songs?page=1&amount=3")[0], "Список пісень")
expect(400, request("GET", "/api/songs?page=0")[0], "Некоректна пагінація")
expect(404, request("GET", "/api/songs/00000000-0000-0000-0000-000000000000")[0], "Неіснуюча пісня")
status, created = request("POST", "/api/songs", song)
expect(201, status, "Створення пісні")
song_id = created["id"]
expect(200, request("GET", "/api/songs/" + song_id)[0], "Читання пісні")
updated = dict(song, title="Тестова українська пісня (оновлена)")
expect(204, request("PUT", "/api/songs/" + song_id, updated)[0], "Оновлення пісні")
expect(204, request("DELETE", "/api/songs/" + song_id)[0], "Видалення пісні")
expect(404, request("GET", "/api/songs/" + song_id)[0], "Пісня після видалення")

expect(200, request("GET", "/api/performances?page=1&amount=3")[0], "Список виступів")
expect(400, request("GET", "/api/performances?amount=0")[0], "Пагінація виступів")
performance = {"participantDatabaseId": 1, "songDatabaseId": 1, "score": 81.5,
               "isCompleted": False}
status, created = request("POST", "/api/performances", performance)
expect(201, status, "Створення виступу")
performance_id = created["id"]
expect(200, request("GET", "/api/performances/" + performance_id)[0], "Читання виступу")
expect(204, request("PUT", "/api/performances/" + performance_id,
                   dict(performance, score=92.0, isCompleted=True))[0], "Оновлення виступу")
expect(204, request("DELETE", "/api/performances/" + performance_id)[0], "Видалення виступу")
expect(404, request("GET", "/api/performances/" + performance_id)[0], "Виступ після видалення")

output = Path("docs/Lab4_http_results.json")
output.write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"Перевірено {len(results)} HTTP-запитів. Результати: {output}")
