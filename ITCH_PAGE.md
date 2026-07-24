# COUNT DOWN

Keep your aim on the opponent until their countdown reaches zero, while moving
out of the enemy's line of fire. Each fighter has three health points.
The player rolls a countdown from 3 to 7. Stage-one enemies roll from 10 to 15,
with that range dropping toward 3 to 7 as stages rise.
Stage one begins with two enemies. Each cleared stage adds one more enemy and
restores the player's health.

## Desktop controls

- `WASD`: Move
- Mouse: The fighter turns toward the cursor
- Hold right mouse button: Aim and advance the countdown
- Bash activates automatically when an enemy enters range
- `R`: Restart after the result
- `Escape`: Quit

## Mobile controls

- Drag from a touch point on the left half: Move
- Hold and drag from a touch point on the right half: Face and aim
- Tap after the result: Restart

The distance from each initial touch point controls stick strength.

## Gamepad controls

- Left stick: Move
- Right stick: Aim direction
- Hold right trigger: Aim and advance the countdown
- Bash activates automatically when an enemy enters range

## Latest update

- Added a ground ring showing the player's melee range and its 15-second
  cooldown progress.
- Melee now locks in enemies already in range, includes their body radius, and
  knocks hit enemies away while they flash.
- Enemies stop moving during their final one-second firing countdown and show
  a placeholder warning marker over the muzzle for the final 0.5 seconds.
- Enemies now dodge only when a player projectile gets close instead of
  reacting immediately to the player's aim line.
- Aiming now gently zooms the camera in and darkens the screen edges for
  immediate visual feedback.
- Added escalating stages that add one enemy after every clear.
- Split countdown ranges: player 3-7, while enemies scale from 10-15 toward 3-7.
- Added automatic click, touch, and gamepad input-mode switching.
- Added dual-stick gamepad controls with right-trigger aim.
- Replaced instant hitscan shots with visible, dodgeable projectiles.
- Increased projectile travel speed by 30%, from 9 to 11.7 meters per second.
- Removed Unity branding from the startup splash and WebGL loading page.
- Expanded the arena to twice its previous width and depth.
- Releasing aim now grants 2x movement speed for 0.5 seconds as an emergency dodge.
- Updated the camera to follow the player throughout the larger arena.
- Player aiming is now instantaneous with no interpolation or turn delay.
- Aim now snaps to an enemy's center inside a 20-degree assist cone, preferring
  the nearer target when silhouettes overlap.
- Enemies now strafe continuously and actively leave the player's firing line,
  making aim denial a consistent way to delay the countdown.
- Living fighters maintain physical separation and can no longer overlap.
- Acquiring a valid aim immediately banks 0.2 seconds of countdown progress,
  and progress continues for a 0.5-second grace window after the target escapes.
- Every countdown tick always takes one second. Enemies begin with high 10-15
  counts and slow movement, then gain movement speed and roll lower starting
  counts as the stage number rises, down to a 3-7 floor.
- Stage one now begins with two enemies.
- Bash now triggers automatically whenever its 15-second cooldown is ready and
  an enemy enters range, hitting every enemy within its 1.8-meter area.
- The desktop crosshair exactly matches the current mouse position.
- Mobile aim directions are applied immediately.
- Moved the arena rails into the background render layer with the floor.
- Anchored the build version to the display safe area's upper-right corner.
- Added the build version to the upper-right corner.
- Made the upper-right build version independent of browser-installed fonts.
- Forced the arena floor behind all gameplay objects in WebGL.
- Fixed fighters appearing buried in or hidden by the floor.
