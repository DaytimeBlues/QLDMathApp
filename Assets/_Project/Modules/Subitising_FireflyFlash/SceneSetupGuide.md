# NERV Mission: Angel Intercept (Subitising)
This module involves scanning the interception field for Angel patterns.

## Scene Hierarchy

```
FireflyFlashScene
├── GameSystem (Empty GameObject)
│   ├── GameManager.cs
│   ├── InteractionController.cs
│   ├── DataService.cs
│   └── ProgressionService.cs
│
├── FireflyFlashGame (Empty GameObject)
│   ├── FireflyFlashController.cs
│   ├── FeedbackSystem.cs
│   ├── NumberAudioService.cs
│   └── AudioSource (component)
│
├── Canvas (Screen Space - Overlay)
│   ├── JarContainer (Image - the glass jar)
│   │   ├── CanvasGroup (for fade effects)
│   │   └── FireflySpawner.cs
│   │
│   ├── AnswerButtonGroup (Horizontal Layout Group)
│   │   ├── AnswerButton_1 (see prefab setup below)
│   │   ├── AnswerButton_2
│   │   └── AnswerButton_3
│   │
│   ├── CharacterGuide (Image - the mascot)
│   │   └── Animator (with Celebrate/Think triggers)
│   │
│   └── ScreenFlash (Full-screen Image, alpha=0)
│       └── CanvasGroup
│
├── ParticleSystem_Confetti
└── ParticleSystem_Sparkles
```

## Prefab: AnswerButton

```
AnswerButton (RectTransform: 120x120 min)
├── Image (Button Background - cream/clay color)
├── Shadow (Image - offset by 4px down, darker)
├── NumberText (TMP - large font, centered)
├── Button (component)
└── AnswerButton.cs
```

### Neo-Skeuomorphic Styling
- **Background Color**: `#F2D9B8` (warm cream)
- **Shadow Color**: `#C4A882` (darker cream)
- **Border Radius**: Use UI Rounded Corners shader or sprite slicing
- **Font**: Sassoon Primary or similar infant-friendly typeface

## Prefab: Firefly

```
Firefly (SpriteRenderer)
├── Sprite: Glowing orb (yellow/green)
├── FireflyAnimator.cs
└── Point Light 2D (optional, for glow effect)
```

## Creating MathProblemSO Assets

1. Right-click in Project → Create → Education → MathProblem
2. Configure each asset:

| Asset Name | Correct Value | Distractors | Difficulty |
|------------|---------------|-------------|------------|
| SUB_L1_001 | 2 | [1, 3] | 0.1 |
| SUB_L1_002 | 3 | [2, 4] | 0.2 |
| SUB_L1_003 | 4 | [3, 5] | 0.3 |
| SUB_L2_001 | 5 | [4, 6] | 0.5 |
| SUB_L2_002 | 6 | [5, 7] | 0.6 |

3. Record Australian-accented audio for each:
   - `questionAudio`: "How many fireflies can you see?"
   - `explanationAudio`: "Let me help you count. One... two... three!"

## Wiring1. Open the `AngelIntercept` scene.
2. Ensure the `AngelInterceptController` has a `NERVTheme` assigned.
3. The `AngelSpawner` should reference the Angel prefab.
   - Drag `AnswerButtonGroup` 
   - Drag `AudioSource`
   - Drag `JarContainer`'s CanvasGroup

2. **FeedbackSystem**:
   - Assign particle systems
   - Assign success/encouragement audio clips
   - Assign character animator

## Testing Checklist

- [ ] Fireflies spawn in dice pattern
- [ ] Fireflies fade after display time
- [ ] Tap button → visual press effect
- [ ] Correct answer → confetti + chime
- [ ] Wrong answer → explanation sequence plays
- [ ] Difficulty adapts after 3-5 attempts
