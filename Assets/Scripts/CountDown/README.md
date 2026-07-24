# COUNT DOWN runtime

The game builds itself when `Main.unity` starts. No scene wiring or external
assets are required.

Optional replacement assets are loaded from these `Resources` paths:

- `CountDown/Materials/Player`
- `CountDown/Materials/Enemy`
- `CountDown/Materials/Gun`
- `CountDown/Materials/Floor`
- `CountDown/Materials/MuzzleFlash`
- `CountDown/Sprites/PlayerDefault`
- `CountDown/Sprites/EnemyDefault`
- `CountDown/Audio/Click`
- `CountDown/Audio/Warning`
- `CountDown/Audio/Gunshot`
- `CountDown/Audio/Impact`
- `CountDown/Audio/MeleeHit`
- `CountDown/Audio/MeleeMiss`
- `CountDown/Audio/Win`
- `CountDown/Audio/Lose`

Material and audio replacements require no code changes. Sprite replacements
should be imported as Sprite (2D and UI); their height is normalized
automatically.

Controls: WASD move, hold right mouse to aim, Space bash, R restart after a
result, Escape quit. The left mouse button is deliberately unused.
