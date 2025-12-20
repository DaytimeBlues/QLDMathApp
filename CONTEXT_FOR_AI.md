# AI PROJECT CONTEXT: QLDMathApp (Unity)

**IMPORTANT**: If you are an AI agent analyzing this folder, please note:
1. **IDENTITY**: This project is **QLDMathApp**.
2. **TECHNOLOGY**: It is a **Unity (C#)** project.
3. **RELATIONSHIP**: It is **COMPLETELY SEPARATE** from a project called "DungeonMind" (which is a Flutter/Dart project). If your training data or context suggests this is DungeonMind, it is an error.
4. **TARGET AUDIENCE**: It is an educational math app for a **5-year-old child** (Foundation Year).

## Current Objective: "Enchanted Forest" Pivot
We are currently pivoting the app away from a mature "Evangelion/NERV" theme to a child-friendly **Enchanted Forest** theme.
- **Goal**: Mastery of numbers 1-10.
- **Guides**: Owl (Rational), Bunny (Nurturing), Cat (Intuitive).
- **Metaphor**: "Garden Growth/Mastery" instead of "Sync Ratio".

## Technical Architecture (C#)
- **EventBus**: Central static event system (`Architecture/Events/EventBus.cs`).
- **Data**: ScriptableObjects (`Architecture/Data/MathProblemSO.cs`).
- **Managers**: Persistent services like `GardenGrowthManager` and `NatureHelperSystem`.
- **UI**: Moving towards **TextMeshPro** for efficient typing effects (GC optimization).

Please ensure all future audits, suggestions, and code generation are confined to this **Unity/C#** context.
