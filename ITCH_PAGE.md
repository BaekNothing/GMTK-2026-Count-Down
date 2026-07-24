# COUNT DOWN

Keep your aim on the opponent until their countdown reaches zero, while moving
out of the enemy's line of fire. Each fighter has three health points.
The player rolls a countdown from 3 to 7, while enemies roll from 5 to 15.
Each cleared stage adds one more enemy and restores the player's health.

## Desktop controls

- `WASD`: Move
- Mouse: The fighter turns toward the cursor
- Hold right mouse button: Aim and advance the countdown
- `Space`: Bash at close range
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
- Left bumper: Bash at close range

## Latest update

- Added escalating stages that add one enemy after every clear.
- Split countdown ranges: player 3-7, enemies 5-15.
- Added automatic click, touch, and gamepad input-mode switching.
- Added dual-stick gamepad controls with right-trigger aim and left-bumper bash.
- Replaced instant hitscan shots with visible, dodgeable projectiles.
- Removed Unity branding from the startup splash and WebGL loading page.
- Expanded the arena to twice its previous width and depth.
- Releasing aim now grants 2x movement speed for 0.5 seconds as an emergency dodge.
- Updated the camera to follow the player throughout the larger arena.
- Player aiming is now instantaneous with no interpolation or turn delay.
- Aim now snaps to an enemy's center inside a 10-degree assist cone, preferring
  the nearer target when silhouettes overlap.
- Enemies now strafe continuously and actively leave the player's firing line,
  making aim denial a consistent way to delay the countdown.
- Living fighters maintain physical separation and can no longer overlap.
- The desktop crosshair exactly matches the current mouse position.
- Mobile aim directions are applied immediately.
- Moved the arena rails into the background render layer with the floor.
- Anchored the build version to the display safe area's upper-right corner.
- Added the build version to the upper-right corner.
- Made the upper-right build version independent of browser-installed fonts.
- Forced the arena floor behind all gameplay objects in WebGL.
- Fixed fighters appearing buried in or hidden by the floor.
