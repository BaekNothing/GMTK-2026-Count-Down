# Startup guide image slots

Place optional transparent PNG files here using these exact names:

- `Move.png`
- `Dash.png`
- `Melee.png`

Unity loads them from `Resources/CountDown/Guide`. The game scales each image
to fit its section frame and draws the localized section label above it.

The sprite-guide generator creates framed drawing guides for all three files.
They are 384 x 216 and may be repainted in place without changing names or size.
