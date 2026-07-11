# ScriptA1

.NET 8 Windows Forms automation tool with local PaddleOCR model inference.

## Local model files

The OCR model files are required at runtime but are not committed to Git because they are large.

After cloning the repository, restore them from the archive in the repository root:

```powershell
New-Item -ItemType Directory -Force inference | Out-Null
tar -xf inference.rar -C inference
```

The expected runtime model directory is:

```text
inference/
```

This directory is intentionally ignored by Git. Do not commit extracted model files or generated build output under `bin/` or `obj/`.

## Main areas

- `Forms/` - WinForms UI screens.
- `Features/Cube/` - cube/property OCR workflow.
- `Features/Market/` - market item OCR workflow.
- `Capture/` - screen and game-window capture helpers.
- `Automation/` - mouse/keyboard/window-message automation helpers.
- `Ocr/` - Paddle runtime and OCR service helpers.
- `Interop/` - Win32 interop wrappers.
