# COUNT DOWN

Keep your aim on the opponent until their countdown reaches zero, while moving
out of the enemy's line of fire. Each fighter has three health points.

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

## Latest update

- Player aiming is now instantaneous with no interpolation or turn delay.
- The desktop crosshair exactly matches the current mouse position.
- Mobile aim directions are applied immediately.
- Moved the arena rails into the background render layer with the floor.
- Anchored the build version to the display safe area's upper-right corner.
- Added the build version to the upper-right corner.
- Made the upper-right build version independent of browser-installed fonts.
- Forced the arena floor behind all gameplay objects in WebGL.
- Fixed fighters appearing buried in or hidden by the floor.
