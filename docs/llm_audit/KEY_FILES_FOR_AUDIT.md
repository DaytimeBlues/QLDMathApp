# QLDMathApp - Key Files for LLM Audit

## Assembly Definitions (.asmdef)
```
Assets/_Project/Architecture/QLDMathApp.Architecture.asmdef
Assets/_Project/Modules/QLDMathApp.Modules.asmdef
Assets/_Project/Bootstrap/QLDMathApp.Bootstrap.asmdef
Assets/_Project/UI/QLDMathApp.UI.asmdef
Assets/_Project/Editor/QLDMathApp.Editor.asmdef
```

## Core Architecture

### Services (Architecture/Services/)
- `PersistenceService.cs` - JSON file persistence with batched saves
- `AccessibilitySettingsService.cs` - Zen Mode, High Contrast, Reduced Motion
- `DataService.cs` - Stealth assessment logging
- `JsonInteractionStore.cs` - NDJSON append-only interaction logs
- `ProgressionService.cs` - Difficulty adjustment engine

### Events (Architecture/Events/)
- `EventBus.cs` - Static event channels for decoupling

### Data (Architecture/Data/)
- `ContentRegistrySO.cs` - Central problem catalog
- `MathProblemSO.cs` - ScriptableObject for math problems

### Audio (Architecture/Audio/)
- `AudioQueueService.cs` - Non-overlapping voice-over queue
- `TTSService.cs` - Text-to-Speech wrapper

### UI Theme (Architecture/UI/)
- `ForestTheme.cs` - Enchanted Forest color palette

## Game Modules

### Subitising (Modules/Subitising_FireflyFlash/Scripts/)
- `FireflyGameLoop.cs` - Main game controller
- `FireflySpawner.cs` - Object pooling for fireflies
- `AnswerButton.cs` - Interactive answer buttons
- `AnswerButtonGroup.cs` - Button group controller

### Counting (Modules/Counting_LunchboxPacker/Scripts/)
- `DraggableItem.cs` - Drag-and-drop with cached raycasts
- `LunchboxSlot.cs` - Drop target slots
- `ForestLunchboxController.cs` - Game controller

### Patterns (Modules/Patterns_PatternBuilder/Scripts/)
- `PatternFlower.cs` - Pattern piece component
- `PatternFlowerController.cs` - Pattern game logic

## UI Layer

### Main Menu (UI/MainMenu/)
- `MainMenuController.cs` - Legacy menu controller
- `MainMenuControllerV2.cs` - Decoupled event-based menu
- `SettingsPanel.cs` - Accessibility settings UI
- `SettingsPanelV2.cs` - Event-driven settings panel

### Dashboard (UI/Dashboard/)
- `ParentDashboard.cs` - Learning analytics view

### Services (UI/Services/)
- `SceneTransitioner.cs` - Async scene loading with overlay
- `AppRoot.cs` - Persistent service container

### Theme (UI/)
- `ThemeApplier.cs` - High contrast mode handler

## Bootstrap

### Bootstrap/
- `AppBootstrapper.cs` - Service initialization and scene loading

## Editor Tools

### Editor/
- `AgenticQualityAssurance.cs` - Automated code quality checks
- `VerticalSliceSetupTool.cs` - Scene setup automation
- `PrefabSetupWizard.cs` - Prefab generation
- `SuperAppSetupWizard.cs` - AppRoot prefab creation

---
**Total: ~35 key files across 6 assemblies**
