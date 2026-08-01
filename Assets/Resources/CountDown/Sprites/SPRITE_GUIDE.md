# COUNT DOWN sprite drawing guide

Edit the two PNG sheets in this folder without changing their canvas size.
Unity slices them automatically from the existing import metadata.

## Sheet layout

- `PlayerSheet.png`: 512 x 640, 4 columns x 5 rows
  - row 1: Idle
  - row 2: Move
  - row 3: Death
  - row 4: Melee
  - row 5: MeleeRecover (the visible post-hit delay/recovery)
- `EnemySheet.png`: 512 x 384, 4 columns x 3 rows
  - row 1: Idle
  - row 2: Move
  - row 3: Death
- `FuseSheet.png`: 384 x 64, 6 columns x 1 row
  - column 1: one horizontal fuse segment
  - columns 2-4: Flame frames 0-2
  - columns 5-6: Alert/exclamation frames 0-1

Rows are listed top-to-bottom. Frames run left-to-right.

## Drawing contract

- Every frame is exactly 128 x 128 pixels.
- Keep the canvas size and the 4-column row order unchanged.
- The vertical white line is character center (`x = 64`).
- The horizontal white line is the foot baseline (`y = 12` from the bottom).
- Keep important art inside the outer 8-pixel safe margin.
- Clear the colored guide pixels when the final art is ready; transparency is supported.
- All gameplay frames face the camera. Put the character's weapon/attack direction
  toward screen-right; runtime flips the sheet when appropriate.
- Idle, Move, Melee, and MeleeRecover loop through all 4 frames.
- Death plays frames 0 through 3 once and holds frame 3.
- Use nearest-neighbor/pixel-preserving resizing if you draft at another resolution.
- Fuse cells are exactly 64 x 64. Keep the segment horizontal and centered;
  runtime rotates and chains six copies of it.

The game loads the named sub-sprites from these sheets. No Animator Controller,
animation clips, prefab wiring, or filename changes are required after repainting.

## Environment and background slots

- `FloorTile.png`: repeating 256 x 256 arena floor texture.
- `WallTile.png`: repeating 256 x 256 boundary wall/rail texture.
- `FloorMark.png`: transparent repeating floor-line texture.
- `BackgroundFar.png`: wide opaque distant sky/forest panorama.
- `BackgroundNear.png`: wide transparent near-forest silhouette.

The far and near backgrounds are placed as separate cards around the arena.
Keep important silhouettes away from the extreme top and side edges. Far art
may be opaque; near art should keep alpha around its treetops.

## Weapon, targeting, combat, and UI slots

The following 128 x 128 transparent frame-guide PNGs provide the exact replacement
filenames: `Gun`, `Stand`, `AimLine`, `AimLock`, `Projectile`,
`ProjectileTrail`, `MuzzleFlash`, `EnemyWarning`, `MeleeRange`,
`MeleeCooldown`, `Crosshair`, `HealthPip`, and `TouchStick`.

Panel textures are `GuidePanel.png` (512 square), `HudPanel.png` (256 x 128),
`Button.png` (192 x 96), and `AimVignette.png` (512 square). Preserve alpha.
The guide generator deliberately keeps these as grids, safe-area frames, and
simple envelope silhouettes so finished art can replace
files without changing code. `GuidePanel`, `Crosshair`, `HealthPip`,
`AimVignette`, `FloorTile`, `WallTile`, `FloorMark`, and `AimLine` are already
loaded by the runtime; the remaining named slots document every currently
procedural visual and are ready for final-art hookup. These are drawing guides,
not generated final resources.

## Regeneration

Run `COUNT DOWN > Regenerate Sprite Guides` in Unity. This regenerates every
non-character frame guide while preserving existing character and fuse sheets.
