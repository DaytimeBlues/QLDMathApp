---
description: How to reset Unity project when compilation errors persist
---
# Unity Project Reset Workflow

Use this workflow when you see cascading "type or namespace not found" errors.

## Step 1: Close Unity
Close the Unity Editor completely before proceeding.

## Step 2: Delete Cache Folders
Delete these folders from your project root:
- `Library/`
- `obj/`
- `Temp/` (if exists)

In PowerShell:
```powershell
Remove-Item -Recurse -Force Library, obj, Temp -ErrorAction SilentlyContinue
```

## Step 3: Reopen Unity
Open Unity and let it regenerate all project files. This takes 1-2 minutes.

## Step 4: Import TextMesh Pro Essentials
1. Go to **Window > TextMeshPro > Import TMP Essential Resources**
2. Click "Import" in the popup dialog
3. Wait for compilation to complete

## Step 5: Verify Assembly Definitions
Ensure all .asmdef files have correct references:
- `QLDMathApp.Modules.asmdef` → needs `Unity.TextMeshPro`
- `QLDMathApp.UI.asmdef` → needs `Unity.TextMeshPro`
- `QLDMathApp.Bootstrap.asmdef` → needs all dependencies

## Step 6: Run Code Quality Check
```
/code-quality
```

## Common Issues

| Error | Cause | Fix |
|---|---|---|
| `MonoBehaviour not found` | Broken Unity cache | Delete Library folder |
| `TMPro not found` | Missing TMP package | Import TMP Essential Resources |
| `Custom script not found` | Compilation order | Check .asmdef references |
| `IEnumerator not found` | Missing namespace | Add `using System.Collections;` |
