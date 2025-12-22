# QLDMathApp: Enchanted Forest (Final State)

## 1. Core Architecture
- **Event-Driven**: Centralized `EventBus` for gameplay, UI, and telemetry.
- **Service-Oriented**: Deterministic initialization via `AppBootstrapper` (Order: -100).
- **Consolidated Services**:
    - `PersistenceService`: Local JSON storage in `persistentDataPath`.
    - `AccessibilitySettingsService`: Unified source of truth for Zen/Contrast/Motion, backed by JSON.
    - `DataService`: Steiner-aligned stealth assessment with high-precision hesitation metrics.
- **Object Pooling**: `FireflySpawner` uses pre-warmed pooling for GC stability on mobile.

## 2. Thematic Rules
- **Themes**: All UI must use the `ForestTheme` Asset.
- **Nomenclature**:
    - NO: Sync, Angel, Eva, Tactical, Intercept, Entry Plug.
    - YES: Garden, Firefly, Forest, Nature, Guide, Picnic.
- **Guides**: Wise Owl, Kind Bunny, Curious Cat.

## 3. Technical Constraints
- **UI Framework**: Vanilla Unity UI + TMPro.
- **Lifecycle**: ALL event subscriptions MUST be in `OnEnable`/`OnDisable`.
- **GC**: NO `Instantiate`/`Destroy` during gameplay. Use the provided pools.
- **Fonts**: Use the provided Google Fonts (Stay away from default Arial).

## 4. Current State
- **Status**: Production Ready.
- **Target**: Year 1 / Foundation Math (Australia).
- **Repo**: High-integrity, zero legacy NERV residues.
