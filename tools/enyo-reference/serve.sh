#!/usr/bin/env bash
# Serves Enyo 1.0 for the reference shooter. The examples load ../../../../1.0/framework/enyo.js with
# launch="debug" (there is no build/ directory), so the server root holds:
#   1.0   -> $ENYO_ROOT          (the framework the examples expect)
#   enyo  -> $ENYO_ROOT          (the examples themselves)
#   pages -> tools/enyo-reference/pages (the single-kind harness)
#
#   tools/enyo-reference/serve.sh [PORT]      default port 8765, ENYO_ROOT defaults to /tmp
set -euo pipefail
here="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
port="${1:-8765}"
enyo="${ENYO_ROOT:-/tmp}"
if [ ! -f "$enyo/framework/enyo.js" ]; then
    echo "No Enyo at $enyo (set ENYO_ROOT)" >&2
    exit 1
fi

root="$(mktemp -d)"
trap 'rm -rf "$root"' EXIT
ln -s "$enyo" "$root/1.0"
ln -s "$enyo" "$root/enyo"
ln -s "$here/pages" "$root/pages"
echo "serving $root on http://127.0.0.1:$port/"
cd "$root"
exec python3 -m http.server "$port" --bind 127.0.0.1
