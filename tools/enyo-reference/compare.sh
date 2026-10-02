#!/usr/bin/env bash
# Shoots a gallery page and its Enyo reference and writes them side by side to artifacts/compare/<Page>.png.
#   tools/enyo-reference/compare.sh <Page> [--scale 1.5]
# Needs serve.sh running, the gallery Desktop head built in Release, and Python with Pillow.
set -euo pipefail
here="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$here/../.." && pwd)"
page="$1"; shift
target="$(awk -F'\t' -v p="$page" '$1 == p { print $2 }' "$here/map.tsv")"
if [ -z "$target" ]; then
    echo "no reference for $page in map.tsv" >&2
    exit 1
fi

out="$root/artifacts/compare"
mkdir -p "$out"
"$here/shoot-reference.sh" "$target" "$out/$page.enyo.png" "$@"
"$root/tools/shoot.sh" gallery "$page" Light Default "$out/$page.avaenyo.png"
python3 - "$out/$page.enyo.png" "$out/$page.avaenyo.png" "$out/$page.png" <<'PY'
import sys
from PIL import Image
a, b = Image.open(sys.argv[1]), Image.open(sys.argv[2])
h = max(a.height, b.height)
canvas = Image.new("RGB", (a.width + b.width + 16, h), (255, 255, 255))
canvas.paste(a, (0, 0))
canvas.paste(b, (a.width + 16, 0))
canvas.save(sys.argv[3])
print("wrote", sys.argv[3])
PY
