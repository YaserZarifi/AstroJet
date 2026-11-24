"""Losslessly repack the installed assets for a smaller local Android test build."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import json
import shutil

root = Path(__file__).resolve().parents[2]
output = root / "AndroidPort" / "packed"
output.mkdir(exist_ok=True)
target = output / "content.zip"
with ZipFile(root / "content.zip") as source, ZipFile(target, "w", ZIP_DEFLATED, compresslevel=6) as packed:
    for entry in source.infolist():
        if entry.is_dir():
            continue
        packed.writestr(entry.filename, source.read(entry))
with ZipFile(root / "content.zip") as source, ZipFile(target) as packed:
    original = {entry.filename: (entry.file_size, entry.CRC) for entry in source.infolist() if not entry.is_dir()}
    result = {entry.filename: (entry.file_size, entry.CRC) for entry in packed.infolist()}
    if original != result:
        raise RuntimeError("Repacked content does not match original entries.")
    broken = packed.testzip()
    if broken:
        raise RuntimeError("Repacked content failed CRC validation: " + broken)
for name in ("music", "shader"):
    shutil.copytree(root / name, output / name, dirs_exist_ok=True)
shutil.copy2(root / "content.txt", output / "content.txt")
report = {
    "files_verified": len(original),
    "original_archive_bytes": (root / "content.zip").stat().st_size,
    "repacked_archive_bytes": target.stat().st_size,
    "validation": "Entry names, sizes and CRCs match; every repacked entry passes decompression and CRC verification.",
}
(output / "packaging-result.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
