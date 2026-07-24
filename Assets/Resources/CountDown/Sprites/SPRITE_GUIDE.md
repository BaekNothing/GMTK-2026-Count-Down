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

The game loads the named sub-sprites from these sheets. No Animator Controller,
animation clips, prefab wiring, or filename changes are required after repainting.
