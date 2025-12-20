---
description: Quality assurance workflow for code changes
---
# Multi-Pass Code Quality Workflow

Before committing ANY code changes, run these passes in order:

## Pass 1: Compiler Check
// turbo
```
dotnet build QLDMathApp.sln --no-restore 2>&1 | head -50
```
If errors appear, fix them before proceeding.

## Pass 2: Brace Balance Check
// turbo
```
pwsh -Command "Get-ChildItem -Path 'Assets/_Project' -Recurse -Filter '*.cs' | ForEach-Object { $o = (Select-String -Path $_.FullName -Pattern '{' -AllMatches).Matches.Count; $c = (Select-String -Path $_.FullName -Pattern '}' -AllMatches).Matches.Count; if ($o -ne $c) { Write-Host \"FAIL: $($_.FullName) - Open: $o, Close: $c\" } }"
```

## Pass 3: Namespace Sanity Check
// turbo
```
grep -rn "MonoBehaviour" Assets/_Project --include="*.cs" | while read line; do file=$(echo "$line" | cut -d: -f1); if ! grep -q "using UnityEngine;" "$file"; then echo "MISSING: $file"; fi; done
```

## Pass 4: Theme Terminology Check
// turbo
```
grep -rnE "NERV|Angel|Tactical|EntryPlug|SyncRatio" Assets/_Project --include="*.cs"
```
If any matches appear, refactor them to Enchanted Forest terminology.

## Pass 5: Self-Review Checklist
Before committing, confirm:
- [ ] Did I accidentally remove any `using` statements?
- [ ] Are all braces balanced (every `{` has a `}`)?
- [ ] Did I introduce any duplicate methods?
- [ ] Is the file's class name matching the filename?
- [ ] Are all event subscriptions in `OnEnable`/`OnDisable`?

## After All Passes
// turbo
```
git add . && git commit -m "Your commit message"
```
