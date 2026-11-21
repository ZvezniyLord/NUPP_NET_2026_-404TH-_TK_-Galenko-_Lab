#!/usr/bin/env bash
# Справжні знімки вікна DB Browser for SQLite зі створеною SQLite-базою.
set -euo pipefail
cd "$(dirname "$0")/.."
db="docs/musiccontest.db"
test -s "$db"

capture() {
  local name="$1"
  shift
  QT_QPA_PLATFORM=xcb sqlitebrowser -R "$@" "$db" &
  local pid=$!
  sleep 5
  local wid
  wid=$(xdotool search --name "DB Browser" | head -n 1 || true)
  if [[ -n "$wid" ]]; then
    xdotool windowsize "$wid" 1480 830 || true
    xdotool windowmove "$wid" 32 24 || true
  fi
  sleep 2
  scrot "docs/Lab3_GUI_${name}.png"
  kill "$pid" || true
  wait "$pid" || true
}
capture "DatabaseStructure"
capture "ContestParticipants" -t ContestParticipants
capture "Songs" -t Songs
capture "Performances" -t Performances
echo "DB Browser screenshots saved from real GUI and database."
