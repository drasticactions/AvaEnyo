#!/bin/sh
# Visual-review helper: renders a gallery page or a demo view to PNG.
#   tools/shoot.sh <gallery|mail|photos> <Page-or-view> <Light|Dark> <Default|Compact> <out.png> [extra args]
# Defaults to 1024x768, the TouchPad size. Build the Desktop heads in Release first.
set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP="$1"; TARGET="$2"; VARIANT="$3"; DENSITY="$4"; OUT="$5"; shift 5
case "$APP" in
  gallery) EXE="$ROOT/samples/AvaEnyo.Gallery.Desktop/bin/Release/net10.0/AvaEnyo.Gallery.Desktop"; SWITCH=--page ;;
  mail)    EXE="$ROOT/samples/Demos/AvaEnyo.Mail.Desktop/bin/Release/net10.0/AvaEnyo.Mail.Desktop"; SWITCH=--view ;;
  photos)  EXE="$ROOT/samples/Demos/AvaEnyo.Photos.Desktop/bin/Release/net10.0/AvaEnyo.Photos.Desktop"; SWITCH=--view ;;
  *) echo "unknown app '$APP' (gallery, mail or photos)" >&2; exit 1 ;;
esac
"$EXE" "$SWITCH" "$TARGET" --variant "$VARIANT" --density "$DENSITY" --size 1024x768 --screenshot "$OUT" "$@"
