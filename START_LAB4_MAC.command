#!/bin/bash
# MusicContest Laboratory 4 - macOS launcher (Apple Silicon and Intel).
set -Eeuo pipefail

cd "$(dirname "$0")"
ROOT="$PWD"
MVC_PID=""
API_PID=""
LOG_DIR="$ROOT/.lab4-logs"
mkdir -p "$LOG_DIR"

fail() {
  printf '\nПомилка: %s\n' "$1" >&2
  printf 'Перевірте журнали в каталозі .lab4-logs.\n' >&2
  exit 1
}

cleanup() {
  printf '\nЗупиняємо сервери MusicContest...\n'
  if [[ -n "$MVC_PID" ]]; then kill "$MVC_PID" 2>/dev/null || true; fi
  if [[ -n "$API_PID" ]]; then kill "$API_PID" 2>/dev/null || true; fi
  wait 2>/dev/null || true
}
trap cleanup EXIT

printf '\n=== MusicContest: лабораторна робота №4 / macOS ===\n'
printf 'Папка проєкту: %s\n' "$ROOT"
[[ -f MusicContest.sln ]] || fail "Не знайдено MusicContest.sln. Запускайте файл з повної копії гілки lab4."

# Пошук інсталяцій .NET в Apple Silicon, Intel і домашньому каталозі.
export PATH="$HOME/.dotnet:/opt/homebrew/bin:/usr/local/bin:$PATH"
if [[ -x "$HOME/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="$HOME/.dotnet"
elif [[ -x /usr/local/share/dotnet/dotnet ]]; then
  export DOTNET_ROOT="/usr/local/share/dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
elif [[ -x /opt/homebrew/share/dotnet/dotnet ]]; then
  export DOTNET_ROOT="/opt/homebrew/share/dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi

has_sdk8() {
  command -v dotnet >/dev/null 2>&1 &&
    dotnet --list-sdks 2>/dev/null | grep -Eq '^8\.'
}

if ! has_sdk8; then
  printf '\n[1/6] Не знайдено .NET 8 SDK.\n'
  printf 'Буде завантажено офіційний інсталятор .NET 8 від Microsoft у ~/.dotnet.\n'
  printf 'Потрібен інтернет. Системні пакети та інші версії SDK не видаляються.\n'
  read -r -p "Продовжити встановлення? [Y/n]: " confirm
  case "$confirm" in n|N|no|NO) fail "Встановлення скасовано.";; esac
  command -v curl >/dev/null 2>&1 || fail "Не знайдено curl."
  installer="$(mktemp "${TMPDIR:-/tmp}/musiccontest-dotnet-install.XXXXXX")" ||
    fail "Не вдалося створити тимчасовий файл."
  if ! curl --fail --location --silent --show-error --retry 2        https://dot.net/v1/dotnet-install.sh -o "$installer"; then
    rm -f "$installer"
    fail "Не вдалося завантажити офіційний інсталятор."
  fi
  if ! bash "$installer" --channel 8.0 --install-dir "$HOME/.dotnet"; then
    rm -f "$installer"
    fail "Не вдалося встановити .NET 8 SDK."
  fi
  rm -f "$installer"
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi
has_sdk8 || fail ".NET 8 SDK не виявлено навіть після встановлення."
printf '\n[1/6] SDK встановлено: '
dotnet --version

printf '\n[2/6] Відновлення пакетів NuGet...\n'
dotnet restore MusicContest.sln || fail "Помилка dotnet restore."

printf '\n[3/6] Збірка рішення...\n'
dotnet build MusicContest.sln -c Release --no-restore ||
  fail "Помилка компіляції. Перевірте вивід вище."

printf '\n[4/6] Підготовка демонстраційної бази SQLite...\n'
dotnet run --project MusicContest.Console -c Release --no-build --no-restore -- --lab3-only ||
  fail "Не вдалося підготувати SQLite."

printf '\n[5/6] Перевірка портів...\n'
for port in 5084 5085; do
  if lsof -nP -iTCP:"$port" -sTCP:LISTEN 2>/dev/null | grep -q LISTEN; then
    fail "Порт $port зайнятий. Закрийте раніше запущені застосунки або звільніть порт."
  fi
done

printf 'Запуск MVC та REST API. Журнали: .lab4-logs/\n'
dotnet "$ROOT/MusicContest.MVC/bin/Release/net8.0/MusicContest.MVC.dll"   --urls http://127.0.0.1:5085 > "$LOG_DIR/mvc.log" 2>&1 &
MVC_PID="$!"
dotnet "$ROOT/MusicContest.REST/bin/Release/net8.0/MusicContest.REST.dll"   --urls http://127.0.0.1:5084 > "$LOG_DIR/api.log" 2>&1 &
API_PID="$!"

wait_for_url() {
  local address="$1"
  local process_id="$2"
  local title="$3"
  local tries
  for ((tries=1; tries<=40; tries++)); do
    if curl --fail --silent --max-time 3 "$address" -o /dev/null; then
      return 0
    fi
    if ! kill -0 "$process_id" 2>/dev/null; then
      return 1
    fi
    sleep 1
  done
  return 1
}

printf '\n[6/6] Очікування відповіді сервера MVC...\n'
if ! wait_for_url http://127.0.0.1:5085/ "$MVC_PID" MVC; then
  tail -n 25 "$LOG_DIR/mvc.log" || true
  fail "MVC не запустився."
fi
printf 'Очікування відповіді REST API...\n'
if ! wait_for_url http://127.0.0.1:5084/swagger/v1/swagger.json "$API_PID" REST; then
  tail -n 25 "$LOG_DIR/api.log" || true
  fail "REST API не запустився."
fi

open http://127.0.0.1:5085/
open http://127.0.0.1:5084/swagger

printf '\nОбидва застосунки працюють:\n'
printf 'MVC:     http://127.0.0.1:5085/\n'
printf 'Swagger: http://127.0.0.1:5084/swagger\n'
printf '\nНе закривайте це вікно під час демонстрації.\n'
printf 'Для зупинки натисніть Control+C.\n'
wait
