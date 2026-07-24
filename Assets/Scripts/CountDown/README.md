# COUNT DOWN runtime

The game builds itself when `Main.unity` starts. No scene wiring or external
assets are required.

Optional replacement assets are loaded from these `Resources` paths:

- `CountDown/Materials/Player`
- `CountDown/Materials/Enemy`
- `CountDown/Materials/Gun`
- `CountDown/Materials/Floor`
- `CountDown/Materials/MuzzleFlash`
- `CountDown/Sprites/PlayerSheet` (4 x 5 frames, 128 px per frame)
- `CountDown/Sprites/EnemySheet` (4 x 3 frames, 128 px per frame)
- `CountDown/Sprites/FuseSheet` (segment, 3 flame frames, 2 alert frames)
- `CountDown/Audio/Click`
- `CountDown/Audio/Warning`
- `CountDown/Audio/Gunshot`
- `CountDown/Audio/Impact`
- `CountDown/Audio/MeleeHit`
- `CountDown/Audio/MeleeMiss`
- `CountDown/Audio/Win`
- `CountDown/Audio/Lose`

Material and audio replacements require no code changes. Repaint the existing
sprite guide sheets in place; their preconfigured named slices drive the runtime
sprite-swap animation. See `Resources/CountDown/Sprites/SPRITE_GUIDE.md`.

Desktop controls: WASD move, use the mouse to choose a facing direction, hold
right mouse to aim and advance the countdown, R restart after a result, and
Escape quit. Bash triggers automatically when an enemy enters range and the
cooldown is ready. The left mouse button is deliberately unused.

Mobile controls: drag anywhere on the left half for analog movement and hold
and drag anywhere on the right half to choose a facing direction. Tap after a
result to restart. Stick input strength is proportional to the distance from
the initial touch point.
