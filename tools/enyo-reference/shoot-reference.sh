#!/usr/bin/env bash
# Screenshots a real Enyo 1.0 page in headless Chromium.
#   tools/enyo-reference/shoot-reference.sh <url-or-kind> <out.png> [--scale 1.5] [--port 8765] [--depends DIR]
# A bare kind (for example widgets.Buttons) is shown through pages/view.html. A path (containing '/') is served as
# is, relative to the server root (for example enyo/support/examples/Theming/onyx/index.html).
# Pages load from a symlinked file root (artifacts/enyo-root: 1.0, enyo and pages, as serve.sh lays out over HTTP),
# because Chromium in some sandboxes cannot reach a local HTTP server. Pass --http to use serve.sh instead.
# A scale of 1.5 exercises Onyx's images-1.5.
set -euo pipefail
target="$1"; out="$2"; shift 2
scale=1; port=8765; depends=""; http=0
while [ $# -gt 0 ]; do
    case "$1" in
        --scale) scale="$2"; shift 2 ;;
        --port) port="$2"; shift 2 ;;
        --depends) depends="$2"; shift 2 ;;
        --http) http=1; shift ;;
        *) echo "unknown argument $1" >&2; exit 1 ;;
    esac
done

here="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
if [ "$http" -eq 1 ]; then
    base="http://127.0.0.1:$port"
else
    # Examples climb four to six folders to reach 1.0/, so 1.0 is linked at two depths.
    top="$(cd "$here/../.." && pwd)/artifacts/enyo-root"
    froot="$top/root"
    enyo="${ENYO_ROOT:-/tmp}"
    mkdir -p "$froot"
    ln -sfn "$enyo" "$top/1.0"
    ln -sfn "$enyo" "$froot/1.0"
    ln -sfn "$enyo" "$froot/enyo"
    ln -sfn "$here/pages" "$froot/pages"
    base="file://$froot"
fi

if [[ "$target" == */* ]]; then
    url="$base/$target"
else
    url="$base/pages/view.html?kind=$target"
    if [ -n "$depends" ]; then
        url="$url&depends=$depends"
    fi
fi

browser="$(command -v chromium || command -v chromium-browser || command -v google-chrome)"
mkdir -p "$(dirname "$out")"
timeout 60 "$browser" --headless --disable-gpu --hide-scrollbars --window-size=1024,768 \
    --force-device-scale-factor="$scale" --timeout=6000 --allow-file-access-from-files \
    --screenshot="$(realpath -m "$out")" "$url" >/dev/null 2>&1
echo "wrote $out from $url"
