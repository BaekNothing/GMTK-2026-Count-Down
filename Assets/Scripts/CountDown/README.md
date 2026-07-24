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

Desktop controls: WASD move, hold right mouse and drag from the click point to
choose a facing direction, Space bash, R restart after a result, Escape quit.
The left mouse button is deliberately unused.

Mobile controls: drag anywhere on the left half for analog movement and hold
and drag anywhere on the right half to choose a facing direction. Tap after a
result to restart. Stick input strength is proportional to the distance from
the initial touch point.
