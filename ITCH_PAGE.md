# COUNT DOWN

Keep aiming until your countdown reaches zero, while moving out of the enemy's
line of fire. Every fighter's countdown advances while they aim, even when the
laser is not touching an opponent. Each fighter has three health points.
The player rolls a countdown from 3 to 7. Stage-one enemies roll from 10 to 15,
with that range dropping toward 3 to 7 as stages rise.
Stage one begins with two enemies. Each cleared stage adds one more enemy and
restores the player's health.

## Desktop controls

- `WASD`: Move
- Mouse: The fighter turns toward the cursor
- Hold right mouse button: Aim and advance the countdown
- Release aim: Dash at double movement speed for 0.5 seconds
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
- Release aim: Dash at double movement speed for 0.5 seconds
- Bash activates automatically when an enemy enters range

## Latest update

- Restored the original background and UI treatment, moved gameplay art to a
  neutral white-and-gray palette, and tightened fuse links into a continuous wick.
- Enlarged the three detailed control-guide images by another 1.3 times and
  moved each title into its own row so it never overlaps the artwork.
- Death animations now play once and hold their final frame.
- Added replaceable framed art guides for the arena floor and walls, layered
  near/far forest backgrounds, gun and aim visuals, combat effects, HUD,
  crosshair, health, touch controls, panels, and all startup guide cards.
- Replaced the player and enemy character sheets with the final hand-drawn
  artwork and slowed character animation playback to 2 FPS.
- Holding aim through a shot now keeps the living target locked even when the
  pointer or stick faces far away. If that target dies, lock transfers by
  virtual-aim proximity, then player distance, with random exact ties.
- While aiming at a locked target, the camera now frames a point 25% of the way
  from the player toward that target.
- Fuse wicks, flames, and alerts now render above floors and walls but below
  player and enemy characters.
- Locked shots now lead a moving target slightly instead of always firing at
  its current center.
- Enemy dodge direction changes now carry momentum, braking for 0.15 seconds
  before accelerating into the new movement over 0.15 seconds.
- Revised Spanish and Portuguese labels so their selections read distinctly,
  and moved Western-language rendering to a dedicated TrueType Latin font.
- Enlarged the guide language button by 1.5x, removed its decorative brackets,
  and added automatic font fitting for constrained button widths.
- The corner help button now keeps exactly the same visual color while pressed.
- Expanded the corner help button hit area to 2.5 times its visible size.
- Guide panels now ignore pointer input: start or resume only from the gray
  backdrop, the footer action, Enter, or gamepad A.
- Added a persistent ten-language selector for English, Korean, Traditional and
  Simplified Chinese, Japanese, French, German, Spanish, Thai, and Portuguese.
  Keyboard or gamepad vertical input moves focus; Enter/A selects; Escape/B or
  the close button cancels. Gamepad Y opens help and then language selection.
- Added a top-right `?` button that pauses combat and reopens the guide at any
  time. Its footer changes from start to resume while the guide is reopened.
- Bundled a WebGL-safe Korean guide font so labels remain visible independently
  of browser-installed fonts.
- Reworked the startup guide into three large image sections for movement,
  dash, and melee. Each section accepts a replaceable resource image and keeps
  its localized title in a separate engine-rendered row above the artwork.
- Tightened every gun fuse so adjacent wick segments remain visually connected.
- Added a three-slot remaining-health display above every enemy.
- Added a localized startup control card with an explicit touch/click/button
  start prompt, opaque content, and a translucent background. Combat waits
  until the player dismisses it.
- The guide calls out the release-to-dash action across mouse, touch, and
  gamepad play.
- Enemy movement speed is 10% lower at every stage.
- Holding aim now locks the first assisted target until aim is released. Moving
  the virtual aim point closer to another enemy transfers the lock, and a
  replaceable translucent target marker identifies the active lock.
- Aim-lock acquisition now uses a tighter 15-degree assist cone.
- Locked shots now recalculate a direct path to the target hitbox center at the
  instant of firing, and player projectiles travel 10% faster.
- Replaced fighter panel-like collision with invisible 3D box hitboxes so
  side-angle shots connect reliably. Enemy hitboxes are 10% larger than their
  visible bodies, while the player's is 10% smaller.
- Character and fuse sprite animations now play at half speed.
- Fuse wicks and flames are twice as large. Initial fuse length now matches the
  starting count, then shrinks smoothly as the countdown advances.
- Player and enemy countdowns now advance whenever they aim, even if the laser
  is not currently touching an opponent.
- The player remains movement-locked for 1.5 seconds after melee.
- Pulling the camera back 15% gives the wide-screen arena more breathing room.
- Melee now has a 1.5-second movement recovery and hits 10% beyond its visible
  trigger range.
- Player projectiles now deal 3 damage; melee and enemy projectiles deal 1.
- Enemy countdown rerolls now bias high when the group is mostly low and bias
  low when the group is mostly high.
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
- Added lightweight sprite-swap animation sheets for player idle, movement,
  death, melee, melee recovery, and enemy idle, movement, and death states.
- Added trailing six-link gun fuses for both sides. The flame burns toward the
  handle as the countdown advances, pauses on an animated warning mark, then fires.
