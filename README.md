# QLD Foundation Math App - Unity Project Setup

This folder typically contains the Unity project. Since this is a generated structure, you need to follow these steps to import it into a real Unity Editor.

## 1. Unity Version
- Recommended: **Unity 2022.3 LTS** or **Unity 6 (2025)**.
- Platform: **Android** (Target API Level 34+ for 2025 devices).

## 2. Dependencies
Open Unity Package Manager and install:
- **Addressables** (com.unity.addressables) - For asset management.
- **Input System** (com.unity.inputsystem) - For touch/drag handling.
- **SQLite** - You will need to drop a `sqlite-net-netstandard.dll` into `Assets/ThirdParty/` for the `DataService` to be fully offline-capable.

## 3. Installation
1. Create a new Unity 2D Core Project.
2. Copy the `Assets` folder from here into your Unity project's root folder.
3. Open Unity. It will compile the new scripts.

## 4. Configuring the Game
1. **Create Data**: Right-click in the Project View -> `Create` -> `Education` -> `MathProblem`. Make a "Problem 1" asset.
2. **Scene Setup**:
   - Create an Empty GameObject "GameSystem".
   - Add `GameManager`, `InteractionController`, `DataService`, and `ProgressionService` components to it.
   - Assign the "Problem 1" asset reference to the test script or Manager if exposed.

## 5. Testing
- Press **Play**.
- Watch the Console for "[GameManager] State: MainMenu".
- The `ProgressionService` logs will show "Mastery Detected" or "Struggle Detected" based on your simulated inputs.

## 6. Architecture Highlights
- **ScriptableObjects**: All math content is in `Architecture/Data`.
- **Observer Pattern**: Check `EventBus.cs` to see how systems talk without referencing each other.
- **Offline-First**: `DataService.cs` has the structure ready for SQLite integration.
