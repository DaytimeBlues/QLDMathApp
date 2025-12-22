# LLM Audit Documentation

This folder contains documentation to help external LLMs understand and audit this Unity mobile math app.

## Quick Start for LLMs

1. **Start here**: [KEY_FILES_FOR_AUDIT.md](KEY_FILES_FOR_AUDIT.md) - Complete list of ~35 key source files
2. **Project context**: [CONTEXT_FOR_AI.md](CONTEXT_FOR_AI.md) - Architecture overview

## Detailed Audit Sections

| File | Contents |
|------|----------|
| [AUDIT_01_Architecture_and_Logic.md](AUDIT_01_Architecture_and_Logic.md) | Core services, EventBus, persistence |
| [AUDIT_02_Gameplay_and_UI.md](AUDIT_02_Gameplay_and_UI.md) | Game modules, UI layer |
| [AUDIT_03_Tools_and_Metadata.md](AUDIT_03_Tools_and_Metadata.md) | Editor tools, .asmdef files |
| [AUDIT_04_Pedagogy_and_Spatial_Logic.md](AUDIT_04_Pedagogy_and_Spatial_Logic.md) | Educational design, ACARA alignment |

## Project Summary

- **Type**: Unity (C#) mobile educational app
- **Target**: Foundation Year students (5-year-olds)
- **Theme**: "Enchanted Forest" (nature-based, child-friendly)
- **Skills**: Subitising, Counting, Patterns

## Source Code

All C# source files are in `Assets/_Project/`:
- `Architecture/` - Core services, events, data
- `Modules/` - Game modules (FireflyFlash, LunchboxPacker, PatternBuilder)
- `UI/` - Main menu, settings, dashboard
- `Bootstrap/` - App initialization
- `Editor/` - Unity editor tools
