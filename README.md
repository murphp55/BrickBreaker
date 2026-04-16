# BrickBreaker (Unity)

## Current state
- A runtime-generated 2D BrickBreaker game built entirely via scripts.
- Objects created at play time: walls, paddle, ball, bricks, HUD.
- Input uses the **New Input System** (UnityEngine.InputSystem).
- Scene does not require manual setup; `BrickBreakerGame` bootstraps itself after scene load.

## How to run
1) Open the project in Unity.
2) Open `Assets/Scenes/SampleScene.unity`.
3) Press Play.

## Controls
- Move: Mouse or A/D or Left/Right arrows
- Launch ball: Space or Left Click
- Restart: R

## Scripts
- `Assets/Scripts/BrickBreakerGame.cs`: Builds the board, walls, HUD, and spawns bricks/ball/paddle.
- `Assets/Scripts/PaddleController.cs`: Paddle movement (mouse/keys).
- `Assets/Scripts/BallController.cs`: Launch, speed maintenance, life loss.
- `Assets/Scripts/Brick.cs`: Brick hit behavior.

## Unity-specific notes
- Input errors were fixed by switching all input reads to `UnityEngine.InputSystem`.
- HUD uses built-in font name `LegacyRuntime.ttf` (Unity 6 change from Arial).
- Rigidbody2D uses `bodyType` (Unity 6 deprecates `isKinematic`).

## Layout / sizing status (important)
- Sizing tweaks were attempted but reverted to the original defaults.
- Current runtime sprites are created from `Texture2D.whiteTexture` using default pixels-per-unit.
- If objects look too small or too large, the likely root cause is the sprite PPU vs world-units mismatch.
- The most reliable fix would be to use actual sprite assets with a known PPU, or explicitly set PPU in code.

## Where we left off
- User reported small sizing in Play; tried runtime PPU tweaks; reverted to defaults.
- User asked how to preview layout before Play; not yet implemented.

## Next options
- Add edit-mode layout preview via `[ExecuteAlways]`.
- Switch to real sprite assets or configure PPU in code with a user-tuned value.
- Add UI and polish (score/lives, win/lose screens, sound, particles).
