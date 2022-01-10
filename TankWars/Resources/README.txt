CHANGE LOG

PS8 - Client Implementation

11/15/2021 - Set up project structure and World wrapper class with members representing JSON objects. Set up initial view (form). Began work on GameController - connection setup and Handshake.

11/16/2021 - Linked the game controller to the view and continued work on the handshake process.

11/17/2021 - Added mouse and key listeners on the view with their respective methods in the controller to maintain MVC.

11/18/2021 - Fixed the asynchronus event loop and added JSON deserialization method.

11/22/2021 - Added drawer methods for Tank, Powerup, and Projectile classes.

11/23/2021 - Added drawer for the walls, OnPaint now draws the background, switched to precompiled network library which has typo in state.ErrorOccured. Needs another r in Occurred :(.

11/24/2021 - Imported all the images as instance variables in the DrawingPanel, made more progress on the WallDrawer method, DrawingPanel now centers view on the player.

11/26/2021 - Added TurretDrawer method for the tank, fixed the WallDrawer method, implemented ProjectileDrawer.

11/27/2021 - Added different color shots depending on tank color, fixed turret orientation when drawn, added checks in the JSON deserialization for complete strings and dead objects.

11/28/2021 - Added and debugged a beam animation for the powerup, removes old beams after animation is over.

11/29/2021 - Added explosion animation on tank death, powerups are now removed after a tank goes over them, added player name, score, and health bar around the players tank, added smoother movement controls.

PS9 - Server Implementation

12/01/2021 - Added Server project and ServerController, set up handshake and XML parsing through settings file and class, began implementing tank collision, updated Model with new properties, methods and constructors

12/02/2021 - Began implementing control commands, projectile collisions, powerup collisions

12/03/2021 - Added random respawn method, beam collisions, functionality to update score on kill, fixes to beam collision and respawn methods.

12/04/2021 - Updated World variables to be read from parsing XML file, fixed tank respawn rate and animations, added collision detection to projectiles

12/06/2021 - Fixed issues with XML reader and respawn collisions with walls. Fixed projectiles impacting dead tanks. Added wraparound from world size constraints to tank movement. Added safe disconnect to clients.

12/07/2021 - Fixed tank death animations on client disconnect, added console notifications on server events, code cleanup and comments.

12/09/2021 - Fixed issue with view not removing disconnected tanks, fixed issue with explosions not animating on beam collision

CLIENT FEATURES

CONTROLS
W - move up
A - move left
S - move down
D - more right

Mouse - aim turret
Left Mouse Button - fire normal shot
Right Mouse Button - fire special beam after powerup has been acquired

Escape - close window

EXTRA FEATURES
- Smooth movement controls. Multiple arrow keys can be pressed at the same time and the controller tracks which buttons have been released yet or are still being held.
- Cool beam animation. The beam uses a gif animation that is played when fired.
- Tank explosion animation. Upon a tanks death, an explosion gif animation is played.
- Multicolored health bar. The health bar changes colors as the tanks health decreases, this makes it easy for the player to know their health.
- Escape key closes the view.

SERVER FEATURES

- World settings can be modified through provided 'Resources/settings.xml' file. In addition to the basic settings, default values can be changed for:
	- Max HP
	- Engine Power
	- Tank Size
	- Projectile Speed
	- Max Powerups
	- Wall Size
	- Max Powerup delay

- Client connects and disconnects are handled gracefully and notifications are sent to the server console.

- Stability tested with multiple connected player and AI clients
