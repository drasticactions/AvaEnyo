#!/usr/bin/env bash
# Screenshots a real Enyo 1.0 page through a sequence of mouse steps, for comparing motion.
#   tools/enyo-reference/shoot-sequence.sh <page> <out-dir> [--probe <js>] <step>...
# <page> is relative to the file root, as for shoot-reference.sh (for example pages/sliding.html).
# Steps (coordinates in a 1024x600 viewport):
#   shot <label>                 screenshot
#   down x,y | move x,y [n] | up x,y | click x,y
#   drag x0,y0 x1,y1 [n]         press and move, without releasing
#   wait <ms>
#   every <ms> <count> <label>   wait and screenshot, count times
#   eval <js>
# --probe <js> is evaluated at each shot and printed next to the file name.
# Needs Node and playwright-core (set PLAYWRIGHT_CORE to its folder if it is not resolvable). Playwright only
# drives the system Chromium over CDP; its own Chromium fails to load the debug loader's many scripts.
#
# Example: drag the right view of pages/sliding.html off and shoot the slide.
#   tools/enyo-reference/shoot-sequence.sh pages/sliding.html artifacts/enyo-slide/dismiss \
#     --probe 'pane.$.right.slidePosition' \
#     'shot start' 'drag 700,300 900,300' 'shot dragging' 'up 900,300' 'every 120 6 slide' 'wait 800' 'shot settled'
set -euo pipefail
here="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$here/../.." && pwd)"
page="$1"; out="$2"; shift 2

top="$root/artifacts/enyo-root"
froot="$top/root"
enyo="${ENYO_ROOT:-/tmp}"
mkdir -p "$froot" "$out"
ln -sfn "$enyo" "$top/1.0"
ln -sfn "$enyo" "$froot/1.0"
ln -sfn "$enyo" "$froot/enyo"
ln -sfn "$here/pages" "$froot/pages"

browser="$(command -v chromium || command -v chromium-browser || command -v google-chrome)"
port="${CDP_PORT:-9333}"
profile="$(mktemp -d)"
"$browser" --headless --disable-gpu --hide-scrollbars --allow-file-access-from-files \
    --remote-debugging-port="$port" --user-data-dir="$profile" about:blank >/dev/null 2>&1 &
pid=$!
trap 'kill "$pid" 2>/dev/null; wait "$pid" 2>/dev/null; rm -rf "$profile" 2>/dev/null || true' EXIT
for _ in $(seq 1 100); do
    curl -sf "http://127.0.0.1:$port/json/version" >/dev/null && break
    sleep 0.1
done

node "$here/shoot-sequence.cjs" "http://127.0.0.1:$port" "file://$froot/$page" "$out" "$@"
